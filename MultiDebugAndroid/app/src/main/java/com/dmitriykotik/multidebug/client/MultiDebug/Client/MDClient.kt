package com.dmitriykotik.multidebug.client.MultiDebug.Client

import kotlinx.coroutines.*
import java.io.*
import java.net.Socket

object MDClient {
    private var socket: Socket? = null
    private var writer: BufferedWriter? = null
    private var reader: BufferedReader? = null
    private var job: Job? = null
    private var authenticated = false

    fun connect(logger: ILogger, host: String, port: Int) {
        job = CoroutineScope(Dispatchers.IO).launch {
            try {
                socket = Socket(host, port)
                writer = BufferedWriter(OutputStreamWriter(socket!!.getOutputStream()))
                reader = BufferedReader(InputStreamReader(socket!!.getInputStream()))

                withContext(Dispatchers.Main) {
                    logger.log(MsgType.None, "[CLIENT] Connected to $host:$port!")
                    logger.onConnect()
                }

                listen(logger)
            } catch (e: Exception) {
                withContext(Dispatchers.Main) {
                    logger.log(MsgType.None, "[CLIENT] Connect error: ${e.message}")
                }
            }
        }
    }

    fun disconnect(logger: ILogger) {
        CoroutineScope(Dispatchers.IO).launch {
            try {
                writer?.close()
                reader?.close()
                socket?.close()

                withContext(Dispatchers.Main) {
                    logger.onDisconnect()
                    logger.log(MsgType.None, "[CLIENT] Disconnected!")
                }
            } catch (e: Exception) {
                withContext(Dispatchers.Main) {
                    logger.log(MsgType.None, "[CLIENT] Disconnect error: ${e.message}")
                }
            }
        }
    }


    fun send(logger: ILogger, content: String) {
        if (content.isBlank()) return
        logger.log(MsgType.None, "> $content")

        when (content.lowercase()) {
            "exit", "disconnect" -> {
                logger.clearInputLine()
                disconnect(logger)
                return
            }
            "cls", "clear" -> {
                logger.clearInputLine()
                logger.logClear()
                return
            }
        }

        CoroutineScope(Dispatchers.IO).launch {
            try {
                writer?.write(content + "\n")
                writer?.flush()
                withContext(Dispatchers.Main) {
                    logger.clearInputLine()
                }
            } catch (e: Exception) {
                withContext(Dispatchers.Main) {
                    logger.log(MsgType.None, "[CLIENT] Send error: ${e.message}")
                }
            }
        }
    }

    private suspend fun listen(logger: ILogger) {
        try {
            while (true) {
                val line = reader?.readLine() ?: break
                val message = SrvMessage.parse(line)

                withContext(Dispatchers.Main) {
                    if (message == null) {
                        logger.log(MsgType.None, "[UNKNOWN] $line")
                        return@withContext
                    }

                    val content = when (message.type) {
                        MsgType.Info -> "[INFO] ${message.content}"
                        MsgType.Debug -> "[DEBUG] ${message.content}"
                        MsgType.Warning -> "[WARNING] ${message.content}"
                        MsgType.Error -> "[ERROR] ${message.content}"
                        MsgType.Passwd -> "[SERVER] Password: "
                        MsgType.None -> message.content
                    }

                    logger.log(message.type, content)

                    if (message.type == MsgType.Error && message.content == "Invalid password") {
                        disconnect(logger)
                    }

                    if (message.type == MsgType.Info && message.content == "Authenticated") {
                        authenticated = true
                    }
                }
            }
        } catch (e: Exception) {
            withContext(Dispatchers.Main) {
                logger.log(MsgType.None, "[CLIENT] Listen error: ${e.message}")
                disconnect(logger)
            }
        }
    }
}
