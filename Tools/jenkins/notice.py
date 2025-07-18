#!/usr/bin/env python  #Pyhon脚本的开头，设置utf-8能保证输入中文不会乱码
#-*- coding:utf-8 -*-

import sys       #用于获取Python脚本外部参数
import requests  #用于发送请求
import time      #时间戳工具
import hashlib   #加密工具
import base64    #base64处理
import hmac      #加密工具
import jenkins   #用于连接Jenkins


# jenkinsUrl = 'http://192.168.8.147:8088/'  #Jenkins服务器地址
# userName = 'streamlab'  #用户名
# password = 'streamlab'  #密码
# #飞书webhook地址 和 签名校验密钥,可在自定义机器人上面获取
# url = 'https://open.feishu.cn/open-apis/bot/v2/hook/b39b5f49-c052-4466-b626-c59a7a856170'
# secret = 'a3L6e1sqk7OfDCECet7Z1f'
timestamp = int(time.time()) #秒级

#通过类的方式实现
class Assistant():
    def __init__ (self):
        #连接Jenkins 服务器
        self.server = jenkins.Jenkins(sys.argv[6], sys.argv[7], sys.argv[8])
        #获取外部部署项目名称
        self.jobName = sys.argv[1]
        #获取分支名
        self.branchName = sys.argv[2]
        #获取版本号
        self.version = sys.argv[3]
        #获取发布版本
        self.development = sys.argv[4]
        #获取资源模式
        self.assetPlayMode = sys.argv[5]
        #获取部署项目版本，即第几次部署
        self.buildNumber = self.server.get_job_info(self.jobName)['nextBuildNumber'] - 1
        #项目部署后的信息
        self.jobInfo = self.server.get_build_info(self.jobName, self.buildNumber)
        #部署结果
        self.result = '成功' if(self.jobInfo['result'] == 'SUCCESS') else '失败'
        #部署后的结果地址
        self.jobUrl = self.jobInfo['url']
        #部署时间
        self.startTime = self.getTime('startTime')
        #部署经历时间
        self.duration = self.getTime('duration')
    # 获取时间相关数据的函数
    def getTime(self,value):
        jobTime = self.jobInfo['timestamp']
        startTime = time.strftime("%Y-%m-%d %H:%M:%S",time.localtime(round(jobTime / 1000)))
        duration = int(round(time.time() - round(jobTime / 1000) ))
        timeObj = {
            'startTime': startTime,
            'duration': duration
        }
        if timeObj[value]:
            return timeObj[value]
    # 签名函数，参照官方文档
    def gen_sign(self):
        # 拼接timestamp和secret
        string_to_sign = '{}\n{}'.format(timestamp, sys.argv[10])
        hmac_code = hmac.new(string_to_sign.encode("utf-8"), digestmod=hashlib.sha256).digest()
        # 对结果进行base64处理
        sign = base64.b64encode(hmac_code).decode('utf-8')
        return sign
        # 消息发送函数
    def sendNotificate(self):
        conStr = '项目名称：{0} \n分支名称：{1}\n版本编号：v{2}\n发布版本：{3}\n资源模式：{4}\n构建编号：第{5}次构建\n开始时间：{6}\n持续时间：{7}秒\n构建结果：{8}\n'
        content = conStr.format(self.jobName,self.branchName,self.version,self.development,self.assetPlayMode,self.buildNumber,self.startTime,self.duration,self.result);
        method = 'post'
        headers = {
            'Content-Type': 'application/json',
        }
        # 飞书接收消息的格式
        json = {
            "timestamp": timestamp,
            "sign": self.gen_sign(),
            "msg_type": "interactive",
            "card": {
                "config": {
                    "wide_screen_mode": True,
                    "enable_forward": True
                },
                "elements": [{
                    "tag": "div",
                    "text": {
                        "content":content,
                        "tag": "lark_md"
                    }
                }, {
                    "actions": [{
                        "tag": "button",
                        "text": {
                            "content": "查看报告",
                            "tag": "lark_md"
                        },
                        "url": self.jobUrl,
                        "type": "default",
                        "value": {}
                    }],
                    "tag": "action"
                }],
                "header": {
                    "title": {
                        "content": self.jobName + " 构建报告",
                        "tag": "plain_text"
                    }
                }
            }
        }

        try:
            res = requests.request(method=method, url=sys.argv[9], headers=headers, json=json)
            print(res.json())
        except ValueError as e:
            print(ValueError)




assistant = Assistant()
assistant.sendNotificate()

