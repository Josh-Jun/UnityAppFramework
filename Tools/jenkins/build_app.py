import os
import sys
import time

static_func = 'App.Editor.View.BuildApp.OneKeyBuild'
# 调用unity中我们封装的静态函数
def call_unity_static_func():
    print('Call Unity Static Func Start')
    log_file = sys.argv[2] + '/unity_build.log'
    print('log_file_path:  ' + log_file)

    # 1. Unity安装路径
    # 2. 项目路径
    # 3. 版本号
    # 4. 是否开发模式
    # 5. 资源播放模式
    # 6. 渠道号
    if sys.platform.startswith('win'):
        # win(cmd)
        cmd = 'start %s -quit -batchmode -projectPath %s -logFile %s -executeMethod %s --version:%s --development:%s --assetplaymold:%s --channel:%s --buildmold:%s'%(sys.argv[1], sys.argv[2], log_file, static_func, sys.argv[3], sys.argv[4], sys.argv[5], sys.argv[6], sys.argv[7])
    else:
        # mac(shell)
        cmd = '%s/Contents/MacOS/Unity -quit -batchmode -projectPath %s -logFile %s -executeMethod %s --version:%s --development:%s --assetplaymold:%s --channel:%s --buildmold:%s'%(sys.argv[1], sys.argv[2], log_file, static_func, sys.argv[3], sys.argv[4], sys.argv[5], sys.argv[6], sys.argv[7])

    print('run cmd:  ' + cmd)
    os.system(cmd)
    print('Call Unity Static Func End')

def monitor_unity_log(target_log):
    pos = 0
    path = sys.argv[2] + '/unity_build.log'
    fp = None
    print('Output Unity Log Start')
    start_output = True
    while start_output:
        if os.path.isfile(path):
            if fp is None:
                fp = open(path, 'r', encoding='utf-8', errors='ignore')

            time.sleep(0.5)
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
    while True:
        if os.path.isfile(path):
            try:
                os.remove(path)
                break
            except PermissionError:
                time.sleep(1)
        else:
            break
    print('Output Unity Log End')


if __name__ == '__main__':
    call_unity_static_func()
    monitor_unity_log('Build Complete!!!')