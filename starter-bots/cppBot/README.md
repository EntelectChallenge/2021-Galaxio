Entelect Challenge c++ bot

Local configuration
Install vcpkg & microsoft signalr client (Windows)
- git clone https://github.com/microsoft/vcpkg ./vcpkg
- cd vcpkg
- .\bootstrap-vcpkg.[bat|sh]
- .\vcpkg install microsoft-signalr

For linux, follow the instructions to install microsoft-signalr on linux, then add the corresponding linked file (same as signalr_client_docker_compile.cmake)

https://github.com/microsoft/vcpkg
https://github.com/aspnet/SignalR-Client-Cpp


