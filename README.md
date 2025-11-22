# GameWatch - Keep track of your in-game time

## Features

* Autosave entry clock
* Rename, reset clock, delete entries
* Adjustable autosave interval; can be disabled entirely too
* Provides way to check if there's an update for the app
* VERY efficient on computer resources
* Human-readable `config.ini` & `entries.ini` files, allowing manual modifications
* Supports utf-8 entry title 🚀

## Platforms supported

* Windows (tested on windows 11) ✅
* Linux (tested on wsl2 + debian trixie) ✅
* Mac/Apple/Darwin (untested but should work) ❓

## How to get the program

* Go to [Releases page](https://github.com/AlexDeFoc/GameWatch/releases) and choose a version
  or just get the latest from [Latest one](https://github.com/AlexDeFoc/GameWatch/releases/latest)
* Download the archive that you wish to use & de-archive it
* Copy the folder which contains the version of the program to your desired place
* You can now run the program, make shortcut on it or anything else!

## Tips for use

Keep the program separate per version, else you will have unused files sometimes or issues with future compatibility.

By keeping the "program" withing its own folder and files separate it will keep everything compact.

### BUT

If you keep the program with its own files what happends with my entries?

*Just copy* the 'entry.ini' & 'config.ini' files from the previous version into the new versioned folder!

### Watch the [DISTRIBUTING](./DISTRIBUTING.md) file for changes

If you want to keep a single folder for the whole program instead of sectioning it per version, it is recommended to watch
the [DISTRIBUTING](./DISTRIBUTING.md) file for changes to what each version is shipped with

## How to build

### Requirements

* Vcpkg
* CMake 4.2.0
* Compiler supporting C++ 23

### Building steps

* Clone the repo
* Configure CMakeLists.txt
* Create a build folder inside the cloned repo
* Go inside it & run `cmake -S .. -B .` and optionally your prefered generator (i use ninja) `-G Ninja`,
  and optionally your prefered compiler if the generator doesn't choose the one you wish for `-DCMAKE_CXX_COMPILER=/path/to/your/compiler`
* In the same folder, run: `cmake --build .`

### Configuring the cmake file

#### NOTE: The first two lines you need to configure

* The cmake toolchain file: This file is created when you have ran `vcpkg integrate install` and needs to be used by the project
* The target triplet: You need to configure it for what you wish to use. The triplets can be found on [This link](https://github.com/microsoft/vcpkg/tree/master/triplets)

## Packaging

Follow the [DISTRIBUTING.md](./DISTRIBUTING.md) file
