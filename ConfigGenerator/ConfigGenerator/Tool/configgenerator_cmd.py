import os
import sys
import argparse
import pathlib
from operator import xor
from typing import AnyStr
from asyncio.windows_events import NULL
import tkinter as tk
import glob
from ConfigGenerator.Tool.git_commit import set_str_config_project,find_list,set_str_source_project,get_str_source_project,get_str_config_project,deploy,copy_tree,find_program,set_list_version_config
from ConfigGenerator.Tool.commit_source import is_repo_clean

configgenerator_dir = os.getcwd()
def check_root_folder(link):
    check_root = False
    url = link.strip()
    #chekc name of configgenerator
    path_folder = os.path.dirname(url)
    config_generator_folder = find_list(path_folder)
    for dir in config_generator_folder:
        if (dir == "CIRO"):
            check_root = True
            set_str_source_project(url)
            print("Check name of folder config: Right")
            break
        else:
            check_root = False
    return check_root

def find_file_config(path_file,bCheck):
    root_list = []
    for(root,folder,list_file) in os.walk(path_file):
        for file_name in list_file:
            if (bCheck == False):
                if (file_name.startswith("ConfigGenerator_")):
                    version_name = (file_name.split("_")[1]).split(".")
                    bCheckDigit = True
                    for checkversion in version_name:
                        if (checkversion.isdigit() == False):
                            bCheckDigit = False
                    if (bCheckDigit):
                        root_list.append(root)
            else:
                if (file_name.startswith("ConfigGenerator_") and file_name.endswith(".exe")):
                    root_list.append(root)
    return root_list

def find_version_config(path_file,bCheck):
    version_list = []
    for(root,folder,list_file) in os.walk(path_file):
        for file_name in list_file:
            if (bCheck == False):
                if (file_name.startswith("ConfigGenerator_")):
                    version_name = (file_name.split("_")[1]).split(".")
                    bCheckDigit = True
                    for checkversion in version_name:
                        if (checkversion.isdigit() == False):
                            bCheckDigit = False
                    if (bCheckDigit):
                        version_name = ".".join(version_name)
                        version_list.append(version_name)
            else:
                if (file_name.startswith("ConfigGenerator_") and file_name.endswith(".exe")):
                    version_name = (file_name.split("_")[1]).split(".")
                    del version_name[len(version_name) - 1]
                    version_name = ".".join(version_name)
                    version_list.append(version_name)
    return version_list

def find_folder_diff(parameter,path):
    listversion = []
    pathfile = []
    exclude = set(['win-64', 'win-x64', 'linux-x64'])
    for(root,folder,file) in os.walk(path, topdown=True):
        folder[:] = [d for d in folder if d not in exclude]
        for file_name in file:
            if (file_name.startswith("ConfigGenerator_") and file_name.endswith(".exe")):
                version_name = (file_name.split("_")[1]).split(".")
                del version_name[len(version_name) - 1]
                version_name = ".".join(version_name)
                listversion.append(version_name)
                pathfile.append(root)

    if (parameter.find("version") != -1):
        return listversion
    elif (parameter.find("path") != -1):
        return pathfile


def set_name_ConfigGenerator(software):
    listfileversion_linux = []
    listversion_linux = []
    listversionwindow = []
    listfileversion_window = []
    list_diff_version = []
    list_diff_path_file = []
    pathConfigRelease = "\\ConfigGenerator\\bin\\Release"
    url = configgenerator_dir.strip()
    configgenerator_dir_root_full = url + pathConfigRelease
    checkFileRelease = pathlib.Path(configgenerator_dir_root_full)
    if (checkFileRelease):
        for (path_file,folder,list_file) in os.walk(configgenerator_dir_root_full):
            if (software == False):
                for folder_name in folder:
                    if (folder_name.find("linux") != -1):
                        dir_root_linux_full = path_file + "\\" + folder_name
                        list_version = find_version_config(dir_root_linux_full,False)
                        list_file = find_file_config(dir_root_linux_full,False)
                        for i in range(len(list_version)):
                            listversion_linux.append(list_version[i])
                        for i in range(len(list_file)):
                            listfileversion_linux.append(list_file[i])
            else:
                for folder_name in folder:
                    if (folder_name.find("win") != -1):
                        dir_root_win_full = path_file + "\\" + folder_name
                        list_version = find_version_config(dir_root_win_full,True)
                        list_file = find_file_config(dir_root_win_full,True)
                        for i in range(len(list_version)):
                            listversionwindow.append(list_version[i])
                        for i in range(len(list_file)):
                            listfileversion_window.append(list_file[i])

        list_diff_version = find_folder_diff("version", configgenerator_dir_root_full)
        list_diff_path_file = find_folder_diff("path", configgenerator_dir_root_full)
        for i in range(len(list_diff_version)):
            listversion_linux.append(list_diff_version[i])
            listversionwindow.append(list_diff_version[i])
        for i in range(len(list_diff_path_file)):
            listfileversion_linux.append(list_diff_path_file[i])
            listfileversion_window.append(list_diff_path_file[i])

        print("LIST OF VERSION:\n")
        if (software == False):
            for i in range(0, len(listversion_linux)):
                print("Version ",i,":",listversion_linux[i])
                print("Path:",listfileversion_linux[i])
                print("\n")
            inputversion = input("Select version config: ")
            set_list_version_config(listversion_linux[int(inputversion)])
            set_str_config_project(listfileversion_linux[int(inputversion)])
        else:
            for i in range(0, len(listversionwindow)):
                print("Version ",i,":",listversionwindow[i])
                print("Path:",listfileversion_window[i])
                print("\n")
            inputversion = input("Select version config: ")
            set_list_version_config(listversionwindow[int(inputversion)])
            set_str_config_project(listfileversion_window[int(inputversion)])
    else:
        print("ConfigGenerator not build")
    return None


def deploy_config_generator(link):
    is_deploy = False
    if((check_root_folder(link)) and is_repo_clean(link)):
        source_project = get_str_source_project()
        toolconfig_project = get_str_config_project()
        print(source_project)
        print("========")
        print(toolconfig_project)
        if (((source_project != NULL) or (toolconfig_project != NULL)) and (source_project != toolconfig_project)):
            bool_deploy = deploy(source_project)
            bool_status = copy_tree(toolconfig_project,source_project,symlinks=False, ignore=None)
            if ((bool_status & bool_deploy) == 1):
                print("Deploy done\n")
                is_deploy = True
            elif ((bool_status == 0) or (bool_deploy == 0)):
                print("Deploy error\n")
    else:
        print("Check folder repo deploy: Error")
    return is_deploy