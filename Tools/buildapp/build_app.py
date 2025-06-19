import os
import sys
import time

static_func = 'App.Editor.View.BuildApp.OneKeyBuild'
# 调用unity中我们封装的静态函数
def call_unity_static_func():
    os.system('git reset HEAD^')
    os.system('git pull')
	log_file = sys.argv[2] + '/unity_build.log'
	cmd = 'start %s -quit -batchmode -projectPath %s -logFile %s -executeMethod %s --version:%s --development:%s --assetplaymold:%s'%(sys.argv[1], sys.argv[2], log_file, static_func, sys.argv[3], sys.argv[4], sys.argv[5])
	print('run cmd:  ' + cmd)
	os.system(cmd)
    os.system('git reset HEAD^')


def monitor_unity_log(target_log):
	pos = 0
	path = sys.argv[2] + '/unity_build.log'
	if os.path.isfile(path):
		os.remove(path)
	fp = None
	print('Start Output Unity Log')
	start_output = True
	while start_output == True:
		if os.path.isfile(path):
			if fp == None:
				fp = open(path, 'r', encoding='utf-8', errors='ignore')
 
		if fp != None:
			fp.seek(pos)
			allLines = fp.readlines()
			pos = fp.tell()
			fp.close()
			fp = None
			for line in allLines:
				print(line)
				if target_log in line:
					start_output = False
					break
		time.sleep(0.5)
	print('End Output Unity Log')


if __name__ == '__main__':
	print('Start BuildApp')
	call_unity_static_func()
	monitor_unity_log('Build Complete!!!')
	print('End BuildApp')