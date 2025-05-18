package com.dmitriykotik.multidebug.client.MultiDebug.Client

interface ILogger {
    fun log(type: MsgType, content: String)
    fun logClear()
    fun onConnect()
    fun onDisconnect()
    fun clearInputLine()
}