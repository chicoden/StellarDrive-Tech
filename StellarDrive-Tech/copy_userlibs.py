import os

USER_ROOT = os.path.expanduser("~")
NUGET_PACKAGES_ROOT = os.path.join(USER_ROOT, ".nuget/packages")
PROJECT_ROOT = __file__.rsplit(os.path.sep, maxsplit=2)[0]
GAME_ROOT = os.path.join(PROJECT_ROOT, "StellarDrive Demo")
USERLIBS_DIR = os.path.join(GAME_ROOT, "UserLibs")

nuget_libs = {
    "wasmtime/48.0.2/lib/netstandard2.1/Wasmtime.Dotnet.dll",
    "wasmtime/48.0.2/runtimes/win-x64/native/wasmtime.dll"
}

for path in nuget_libs:
    lib_name = path[path.rfind("/")+1:]
    with (
        open(os.path.join(NUGET_PACKAGES_ROOT, path), "rb") as source,
        open(os.path.join(USERLIBS_DIR, lib_name), "wb") as dest
    ):
        print("Copying", source.name, "to", dest.name)
        dest.write(source.read())

print("Done copying userlibs")
