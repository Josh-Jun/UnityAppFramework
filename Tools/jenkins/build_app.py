import os
import sys
import time

static_func = 'App.Editor.View.BuildApp.OneKeyBuild'
# 调用unity中我们封装的静态函数
def call_unity_static_func():
    print('Start Call Unity Static Func')
    log_file = sys.argv[2] + '/unity_build.log'
    cmd = 'start %s -quit -batchmode -projectPath %s -logFile %s -executeMethod %s --version:%s --development:%s --assetplaymold:%s'%(sys.argv[1], sys.argv[2], log_file, static_func, sys.argv[3], sys.argv[4], sys.argv[5])
    print('run cmd:  ' + cmd)
    os.system(cmd)
    print('End Call Unity Static Func')

def monitor_unity_log(target_log):
    pos = 0
    path = sys.argv[2] + '/unity_build.log'
    fp = None
    print('Start Output Unity Log')
    start_output = True
    while start_output:
        if os.path.isfile(path):
            if fp is None:
                fp = open(path, 'r', encoding='utf-8', errors='ignore')

        if fp is not None:
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


def run_git_cmd():
    print('Start Run Git Cmd')
    os.system('cd ' + sys.argv[2])
    os.system('git reset HEAD^')
    os.system('git checkout ' + sys.argv[6])
    os.system('git pull')
    print('End Run Git Cmd')


if __name__ == '__main__':
    print('Start BuildApp')
    run_git_cmd()
    call_unity_static_func()
    monitor_unity_log('Build Complete!!!')
    print('End BuildApp')