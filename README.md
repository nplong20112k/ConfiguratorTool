### Project is published as portable for both linux-x64 and win-x64 by command publish.bat

### In order to deploy published files, user run command Deploy_ConfigGenerator.py

#### Process for auto deploy CIRO for product that uses Deploy_ConfigGenerator
A tool was made to correctly copy/paste the files new version CIRO from ConfigGenerator for product, it is **Deploy** script.
When using this tool to copy the artifacts from this repository, the steps are only:

1. Open Windows Terminal or Linux Terminal
2. Move in the directory where the Deploy_ConfigGenerator.py is located
3. Call `python Deploy_ConfigGenerator.py` script.
   For example

    ```sh
    > python Deploy_ConfigGenerator.py -d -c -w -b F:/Alpha1/alpha/tool/CIRO F:/Main_Project/Deledda/deledda/tool/CIRO
    ```
    Witch function --help

    ```sh
    > python Deploy_ConfigGenerator.py -h
    ```
4. Function
- Optional arguments:
  - -h, --help      show this help message and exit
  - -d, --deploy    Deploy CIRO
  - -b,   path      Select "CIRO" folder of one or more source code project, using "space" to differentiate.

    _**Windows Terminate:**_
    F:\Main_Project\Deledda\deledda\tool\CIRO
    F:/Main_Project/Deledda/deledda/tool/CIRO

    _**Linux Terminate:**_
    /F/Main_Project/Deledda/deledda/tool/CIRO
    F:/Main_Project/Deledda/deledda/tool/CIRO

- Deploy option:
  - -c, --commit    Commit sourcode
  - -p, --push      Push commit
  - -w, --windows   Deploy tool on Windows
  - -l, --linux     Deploy tool on Linux


