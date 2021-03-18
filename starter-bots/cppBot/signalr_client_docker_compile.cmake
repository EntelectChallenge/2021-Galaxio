set(SIGNALR_INCLUDE_DIR /opt/signalrclient/include) #IMPORTANT
set(SIGNALR_LIBRARY /opt/signalrclient/build.release/bin/libsignalrclient.so)#IMPORTANT
include_directories(${SIGNALR_INCLUDE_DIR}) #IMPORTANT
link_libraries(${SIGNALR_LIBRARY}) #IMPORTANT