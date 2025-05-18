package com.dmitriykotik.multidebug.client.MultiDebug.Client

data class SrvMessage(val type: MsgType, val content: String) {
    companion object {
        fun parse(line: String): SrvMessage? {
            val idx = line.indexOf(':')
            if (idx < 0) return null

            val typeStr = line.substring(0, idx)
            val content = line.substring(idx + 1)

            return try {
                val type = MsgType.valueOf(typeStr)
                SrvMessage(type, content)
            } catch (_: IllegalArgumentException) {
                null
            }
        }
    }
}