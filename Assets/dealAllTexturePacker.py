#coding:utf-8

import os
import os.path
import sys
import re

def cur_file_dir():
     path = sys.path[0]
     if os.path.isdir(path):
         return path
     elif os.path.isfile(path):
         return os.path.dirname(path)
print cur_file_dir()

rootdir = cur_file_dir()+"/Textures"  


def GetMiddleStr(content,endStr):
    patternStr = r'(.+?)%s$'%(endStr)
    p = re.compile(patternStr,re.IGNORECASE)
    m= re.match(p,content)
    if m:
        print "filename is:" + content
        command ="TexturePacker "+content
        os.system(command)

for parent,dirnames,filenames in os.walk(rootdir):
    for dirname in  dirnames:
        for filename in filenames:
            fileAllPath = os.path.join(parent,filename)
            GetMiddleStr(fileAllPath,"All.tps")