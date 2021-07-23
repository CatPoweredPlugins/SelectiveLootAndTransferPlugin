#!/bin/bash

## uncomment line below if you want target netf ASF version
build_netf=1

################################################################################

PATH=/bin:/sbin:/usr/bin:/usr/sbin:/usr/local/bin:/usr/local/sbin:$HOME/bin
export PATH

################################################################################

_PROGN_=`basename $0`

_INSTDIR_=`dirname $0`
[[ $_INSTDIR_ = . ]] && _INSTDIR_=`pwd`

################################################################################

## getting current directory name from '$_INSTDIR_' variable
plugin_name=$(echo $_INSTDIR_ | sed 's|.*/||')

# download submodule
if [[ ! -d ArchiSteamFarm/ArchiSteamFarm ]]; then
   git submodule update --init
fi

if [[ $# -gt 1 ]]; then
   echo "Too many arguments. Exiting."
   exit 1
elif [[ $# -eq 1 ]]; then
   ## update submodule to required tag as specified in '$1'
   git submodule foreach "git fetch origin; git checkout $1;"
else
   ## otherwise update submodule to latest tag
   git submodule foreach "git fetch origin; git checkout $(git rev-list --tags --max-count=1);"
fi

## print what version we are building for
git submodule foreach "git describe --tags;"

if [[ -d ./out ]]; then
   rm -rf ./out
fi


## hacks to allow building netf
if [[ $build_netf -eq 1 ]]; then
    sed -i 's|<ItemGroup>|<ItemGroup Condition="'\''$(TargetFramework)'\'' == '\''net5.0'\''"><!--hacks-->|' $plugin_name/$plugin_name.csproj
fi

## release generic version
dotnet restore
sync
dotnet publish -c "Release" -f net5.0 -o "out/generic" "/p:LinkDuringPublish=false"
mkdir ./out/$plugin_name
cp ./out/generic/$plugin_name.dll ./out/$plugin_name
7z a -tzip -mx7 ./out/$plugin_name.zip ./out/$plugin_name
rm -rf out/$plugin_name

## hacks to allow building netf
if [[ $build_netf -eq 1 ]]; then
    sed -i 's|<ItemGroup Condition="'\''$(TargetFramework)'\'' == '\''net5.0'\''"><!--hacks-->|<ItemGroup Condition="'\''$(TargetFramework)'\'' == '\''net48'\''"><!--hacks-->|' $plugin_name/$plugin_name.csproj
fi

## release generic-netf version
if [[ $build_netf -eq 1 ]]; then
    dotnet msbuild /m /r /t:Publish /p:Configuration=Release /p:TargetFramework=net48 /p:PublishDir=out/generic-netf /p:ASFNetFramework=true
    mkdir ./out/$plugin_name
    cp ./$plugin_name/out/generic-netf/$plugin_name.dll ./out/$plugin_name
    7z a -tzip -mx7 ./out/$plugin_name-netf.zip ./out/$plugin_name
    rm -rf out/$plugin_name

## hacks to allow building netf
    sed -i 's|<ItemGroup Condition="'\''$(TargetFramework)'\'' == '\''net48'\''"><!--hacks-->|<ItemGroup>|' $plugin_name/$plugin_name.csproj
fi
