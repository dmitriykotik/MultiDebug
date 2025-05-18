package com.dmitriykotik.multidebug.client

import android.os.Bundle
import android.widget.Button
import android.widget.EditText
import android.widget.TextView
import androidx.appcompat.app.AppCompatActivity
import com.dmitriykotik.multidebug.client.MultiDebug.Client.ILogger
import com.dmitriykotik.multidebug.client.MultiDebug.Client.MDClient
import com.dmitriykotik.multidebug.client.MultiDebug.Client.MsgType
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch

class MainActivity : AppCompatActivity(), ILogger {

    private lateinit var ipField: EditText
    private lateinit var portField: EditText
    private lateinit var connectButton: Button
    private lateinit var disconnectButton: Button
    private lateinit var commandField: EditText
    private lateinit var sendButton: Button
    private lateinit var logView: TextView

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContentView(R.layout.activity_main)

        ipField = findViewById(R.id.deviceIP)
        portField = findViewById(R.id.devicePort)
        connectButton = findViewById(R.id.connectButton)
        disconnectButton = findViewById(R.id.disconnectButton)
        commandField = findViewById(R.id.commandInput)
        sendButton = findViewById(R.id.sendButton)
        logView = findViewById(R.id.outputField)

        disconnectButton.isEnabled = false
        commandField.isEnabled = false
        sendButton.isEnabled = false

        connectButton.setOnClickListener {
            val ip = ipField.text.toString()
            val port = portField.text.toString().toIntOrNull()
            if (port != null) {
                CoroutineScope(Dispatchers.IO).launch {
                    MDClient.connect(this@MainActivity, ip, port)
                }
            } else {
                log(MsgType.Error, "Invalid port")
            }
        }

        disconnectButton.setOnClickListener {
            MDClient.disconnect(this)
        }

        sendButton.setOnClickListener {
            val msg = commandField.text.toString()
            CoroutineScope(Dispatchers.IO).launch {
                MDClient.send(this@MainActivity, msg)
            }
        }
    }

    override fun log(type: MsgType, content: String) {
        runOnUiThread {
            logView.append("$content\n")
            val scrollAmount = logView.layout?.getLineTop(logView.lineCount)?.coerceAtLeast(0) ?: 0
            logView.scrollTo(0, scrollAmount)
        }
    }

    override fun logClear() {
        runOnUiThread {
            logView.text = ""
        }
    }

    override fun onConnect() {
        runOnUiThread {
            connectButton.isEnabled = false
            disconnectButton.isEnabled = true
            commandField.isEnabled = true
            sendButton.isEnabled = true
        }
    }

    override fun onDisconnect() {
        runOnUiThread {
            connectButton.isEnabled = true
            disconnectButton.isEnabled = false
            commandField.isEnabled = false
            sendButton.isEnabled = false
        }
    }

    override fun clearInputLine() {
        runOnUiThread {
            commandField.text.clear()
        }
    }
}
