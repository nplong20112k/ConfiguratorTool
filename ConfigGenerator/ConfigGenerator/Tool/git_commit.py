from genericpath import isdir, isfile
from pathlib import Path
from distutils.dir_util import copy_tree
from os import listdir
from os.path import isfile, join
import os
from pickle import FALSE, TRUE
import tkinter as tk
import shutil

root = Path.cwd()
configgenerator_dir = Path(root,'net6.0')
source_full = 0
configgenerator_full = 0
string_name_project = 0
numberversionconfig = 0
list_version = []


def set_list_version(list):
    global list_version
    list_version = list

def get_list_version():
    list = list_version
    return list

def find_list(str):
    only_dir = [f for f in listdir(str) if isdir(join(str, f))]
    return only_dir

def find_program(str):
    file_program = [f for f in os.listdir(str) if os.path.isfile(str+'/'+f)]
    return file_program

def set_str_config_project(string_project):
    global configgenerator_full
    configgenerator_full = string_project

def get_str_config_project():
    string = configgenerator_full
    return string

def set_str_source_project(string_project):
    global source_full
    source_full = string_project

def get_str_source_project():
    string = source_full
    return string

def set_str_name_project(string_name):
    global string_name_project
    string_name_project = string_name

def set_list_version_config(list_version):
    global numberversionconfig
    numberversionconfig = list_version

def get_list_version_config():
    string = numberversionconfig
    return string

def deploy(source_project):
    source_project_version = find_program(source_project)
    for file in source_project_version:
        if ((file.find("ConfigGenerator_") != -1) and (file.endswith(".pptx") != 1)):
            pathfile = source_project + "\\" + file
            os.remove(pathfile)
    if not os.path.exists(source_project):
        os.makedirs(source_project)
    return 1

def copy_tree(config_full, source_project_copy, symlinks=False, ignore=None):
    config_file_version = find_program(config_full)
    version = get_list_version_config()
    print("CONFIG FILE VERSION")
    for item in config_file_version:
        if ((item.startswith("ConfigGenerator_")) and (item.find(version,16) != -1)):
            file_config = os.path.join(config_full,item)
            file_source = os.path.join(source_project_copy,item)
            if os.path.isdir(file_config):
                shutil.copytree(file_config, file_source, symlinks, ignore)
            else:
                shutil.copy2(file_config, file_source)
    return 1
