'use strict';
var rules = require('./rulesmanage').rules;
var linq = require('linq');
var redis = require("redis"),
    RDS_PORT = 6379,
    RDS_HOST = "192.168.1.70",
    RDS_PWD = '',
    PDS_OPTS = { auth_pass: RDS_PWD };

var nodemailer = require('nodemailer');
var transporter = nodemailer.createTransport({
    service: 'qq',
    port: 465, // SMTP �˿�
    secureConnection: true, // ʹ�� SSL
    auth: {
        user: 'lab2401@qq.com',
        //�������벻��qq���룬�������õ�smtp����
        pass: '*****'
    }
});
var admin = "504883942@qq.com";
var mailOptions = {
    from: '768065158@qq.com', // ������ַ
    to: '528779822@qq.com', // �ռ��б�
    subject: 'Hello sir', // ����
    //text��html����ֻ֧��һ��
    text: 'Hello world ?', // ����
    html: '<b>Hello world ?</b>' // html ����
};
Promise.all(rules).then(values => {
    var client = redis.createClient(RDS_PORT, RDS_HOST);

    var list = linq.from(values)
        .where(p => p.type == true)
        .select(p =>
            new Promise((resolve, reject) => {
                client.rpush("message", admin + "," + p.msg, function (err) {
                    if (!err) {
                        resolve(true);
                    }
                });
            }));
    Promise.all(list).then(values => {
        client.end(true);
    });

     


    console.log(values);
});

