
import os
import sys
import argparse
import pathlib
from operator import xor
from typing import AnyStr
from asyncio.windows_events import NULL
import tkinter as tk
import glob
from ConfigGenerator.Tool.commit_source import is_repo_clean,handle_git_commit,handle_git_push
from ConfigGenerator.Tool.configgenerator_cmd import set_name_ConfigGenerator,deploy_config_generator
def _dir_path(string):
    if os.path.isdir(string):
        return string
    else:
        raise NotADirectoryError(string)

def main():

    parser = argparse.ArgumentParser(description='Deploy ConfigGenerator')
    parser.add_argument('-d','--deploy',action='store_true',dest='deploy',default=False,help='Deploy configgenerator')

    parser_option = parser.add_argument_group('Deploy option:')
    parser_type_option = parser_option.add_mutually_exclusive_group()
    parser_type_option.add_argument('-c', '--commit',action='store_true',dest='commit',default=False,help='Commit sourcecode')
    parser_type_option.add_argument('-p', '--push',action='store_true',dest='push',default=False,help='Commit and push ')

    parset_enviroment = parser.add_argument_group('Chose Enviroment:')
    parset_enviroment_type = parset_enviroment.add_mutually_exclusive_group()
    parset_enviroment_type.add_argument('-w', '--windows',action='store_true',dest='windows',default=False,help='Enviroment: Windows')
    parset_enviroment_type.add_argument('-l', '--linux',action='store_true',dest='linux',default=False,help='Enviroment: Linux')

    parser.add_argument('-b',action='store',dest='path',type=str,nargs = '*',help='Select scancode_generator folder of one or more source code project. On Windows Terminal: F:/Alpha1/alpha/tool/CIRO. On Linux Terminal: /F/Alpha1/alpha/tool/CIRO')
    args = parser.parse_args()

    if args.windows:
        args.linux = False
        set_name_ConfigGenerator(True)
    elif args.linux:
        args.windows = False
        set_name_ConfigGenerator(False)

    if args.push:
        args.commit = False
    elif args.commit:
        args.push = False
    if ((args.path) and (args.deploy)):

        if not args.deploy:
            parser.error('Missing `deploy`')
        if not args.path:
            parser.error('Missing path file')
        if ((args.path) and (args.deploy)):
            for i in range(len(args.path)):
                print("Start deploy repo " + str(i+1) + ":")
                print(args.path[i])
                is_deploy = deploy_config_generator(args.path[i])
                if (is_deploy):
                    if (args.commit):
                        handle_git_commit(args.path[i])
                        print("Commit done\n")
                    elif (args.push):
                        handle_git_push(args.path[i])
                        print("Push done\n")

if __name__ == '__main__':
    sys.exit(main())