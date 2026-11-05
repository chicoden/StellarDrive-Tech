import os

USER_ROOT = os.path.expanduser("~")
NUGET_PACKAGES_ROOT = os.path.join(USER_ROOT, ".nuget/packages")
PROJECT_ROOT = __file__.rsplit(os.path.sep, maxsplit=2)[0]
GAME_ROOT = os.path.join(PROJECT_ROOT, "StellarDrive Demo")
USERLIBS_DIR = os.path.join(GAME_ROOT, "UserLibs")

def copy_file(source_path, dest_path):
    print("Copying", source_path, "to", dest_path)
    with (
        open(source_path, "rb") as source,
        open(dest_path, "wb") as dest
    ):
        dest.write(source.read())

def copy_file_to_dir(source_path, dest_path):
    file_name = source_path[source_path.rfind("/")+1:]
    copy_file(source_path, os.path.join(dest_path, file_name))

for path in (
    "wasmtime/48.0.2/lib/netstandard2.1/Wasmtime.Dotnet.dll",
    "wasmtime/48.0.2/runtimes/win-x64/native/wasmtime.dll"
):
    copy_file_to_dir(os.path.join(NUGET_PACKAGES_ROOT, path), USERLIBS_DIR)

copy_file_to_dir(os.path.join(PROJECT_ROOT, "minicc/build/libtcc.dll"), USERLIBS_DIR)

print("Done copying userlibs")
