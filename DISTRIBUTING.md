# Guide to distributing releases on GitHub


# Steps for distributing 

* Grab all binary dependencies
* Create folder with name = tag version
* Archive & compress into .zip/.tar.gz file
* Name the archive the correct format: "architecture\_platform". For example:
    * x64\_windows
    * x86\_windows
    * x64\_linux
    * x64\_darwin

# Shipped files along side the executable program

## V1.0.0

### x64\_windows

* boost\_json-vc143-mt-gd-x64-1\_89.dll
* libcrypto-3-x64.dll
* libssl-3-x64.dll
