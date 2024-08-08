import subprocess
from pathlib import Path
from .git_commit import get_str_config_project
import os
path_project = 0

def set_path_of_project(path):
    global path_project
    path_project = path


def _get_path_of_project():
    path_name = path_project
    return path_name

def get_name_branch(string):
    git_branch  = subprocess.Popen('git rev-parse --abbrev-ref HEAD', cwd= string,shell=True, stdout=subprocess.PIPE).stdout.read().decode().rstrip()
    return git_branch

def _get_current_status_(string):
    status_branch = subprocess.Popen('git status', cwd= string,shell=True, stdout=subprocess.PIPE).stdout.read().decode().rstrip()
    return status_branch

def is_repo_clean(string):
    print('Checking current branch of project:')
    current_status = _get_current_status_(string)
    string_rep = 'nothing to commit, working tree clean'
    print(current_status)
    if (current_status.find(string_rep,8)!= -1):
        return True
    else:
        return False

def _git_add_all(string):
    git_add_all = subprocess.Popen('git add . --all', cwd= string,shell=True, stdout=subprocess.PIPE)
    git_add_all.communicate()
    return None

def _git_add_force(string):
    git_add_force = subprocess.Popen('git add --force *.exe *.pdb *.dll', cwd= string,shell=True, stdout=subprocess.PIPE)
    git_add_force.communicate()
    return None

def _git_commit(string,stringMessenge):
    git_commit = subprocess.Popen('git commit ' + stringMessenge, cwd=string,stdout=subprocess.PIPE)
    git_commit.communicate()
    return None

def _git_current_hash(string):
    git_hash = subprocess.Popen('git rev-parse HEAD', cwd= string,shell=True, stdout=subprocess.PIPE).stdout.read().decode().rstrip()
    return git_hash

def _git_current_name_branch(string):
    git_name = subprocess.Popen('git rev-parse --abbrev-ref HEAD', cwd=string,shell=True, stdout=subprocess.PIPE).stdout.read().decode().rstrip()
    return git_name

def _git_push(string,refKind,refToPush):
    cmd_string = "git push origin refs/" + refKind + "/" + refToPush + ":refs/" + refKind + "/" + refToPush
    git_push = subprocess.Popen(cmd_string, cwd=string,stdout=subprocess.PIPE)
    git_push.communicate()
    return None

def handle_git_commit(string):
    scancode_project = get_str_config_project()
    path_scancode_project = os.path.dirname(scancode_project)
    _git_add_force(string)
    _git_add_all(string)
    current_CSV_repohash = _git_current_hash(path_scancode_project)
    string_messenge = '-m \"Deploy ConfigGenerator from commit # ' + current_CSV_repohash + '\"'
    _git_commit(string,string_messenge)
    return None

def handle_git_push(string):
    scancode_project = get_str_config_project()
    path_scancode_project = os.path.dirname(scancode_project)
    _git_add_force(string)
    _git_add_all(string)
    current_name = _git_current_name_branch(string)
    current_CSV_repohash = _git_current_hash(path_scancode_project)
    string_messenge = '-m \"Deploy ConfigGenerator from commit # ' + current_CSV_repohash + '\"'
    _git_commit(string,string_messenge)
    _git_push(string,'heads',current_name)
    return None
