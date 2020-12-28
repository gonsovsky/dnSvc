#!/bin/sh

./buildpkg.sh
nuget push SmbLibraryStd.`cat version.txt`.nupkg -Source nuget.org
