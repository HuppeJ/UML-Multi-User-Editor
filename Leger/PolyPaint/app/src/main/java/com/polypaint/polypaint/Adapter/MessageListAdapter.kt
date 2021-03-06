package com.polypaint.polypaint.Adapter

import android.content.Context
import android.graphics.Color
import android.graphics.PorterDuff
import android.graphics.drawable.Drawable
import androidx.core.content.ContextCompat
import androidx.recyclerview.widget.RecyclerView
import android.text.format.DateUtils
import android.util.Log
import android.view.LayoutInflater
import android.view.View
import android.view.ViewGroup
import android.widget.TextView
import com.polypaint.polypaint.Model.Message
import com.polypaint.polypaint.Model.User
import com.polypaint.polypaint.R
import java.text.SimpleDateFormat

class MessageListAdapter (var context: Context, var messageList: List<Message>, var user: String) : androidx.recyclerview.widget.RecyclerView.Adapter<androidx.recyclerview.widget.RecyclerView.ViewHolder>(){

    companion object {
        private const val VIEW_TYPE_MESSAGE_SENT = 1
        private const val VIEW_TYPE_MESSAGE_RECEIVED = 2
    }

    override fun onCreateViewHolder(parent: ViewGroup, viewType: Int): androidx.recyclerview.widget.RecyclerView.ViewHolder {
        val view: View

        return if (viewType == VIEW_TYPE_MESSAGE_SENT) {
            view = LayoutInflater.from(parent.context).inflate(R.layout.item_message_sent, parent, false)
            SentMessageHolder(view)
        } else {
            view = LayoutInflater.from(parent.context).inflate(R.layout.item_message_received, parent, false)
            ReceivedMessageHolder(view)
        }
    }

    override fun getItemCount(): Int {
        return messageList.size
    }

    override fun onBindViewHolder(holder: androidx.recyclerview.widget.RecyclerView.ViewHolder, position: Int) {
        val message = messageList[position]

        when (holder.itemViewType) {
            VIEW_TYPE_MESSAGE_SENT -> (holder as SentMessageHolder).bind(message)
            VIEW_TYPE_MESSAGE_RECEIVED -> (holder as ReceivedMessageHolder).bind(message)
        }
    }

    override fun getItemViewType(position: Int): Int {
        val message = messageList[position]

        return if (message.sender == user) {
            VIEW_TYPE_MESSAGE_SENT
        } else {
            VIEW_TYPE_MESSAGE_RECEIVED
        }
    }

    private inner class SentMessageHolder internal constructor(itemView: View) : androidx.recyclerview.widget.RecyclerView.ViewHolder(itemView) {
        internal var messageText: TextView = itemView.findViewById(R.id.text_message_body) as TextView
        internal var timeText: TextView = itemView.findViewById(R.id.text_message_time) as TextView


        internal fun bind(message: Message) {
            //var rectangle: Drawable? = messageText.background
            //rectangle?.mutate()?.setColorFilter(R.color.colorOutlineSendMessage, PorterDuff.Mode.SRC_ATOP)
            messageText.text = message.text
            val formatter = SimpleDateFormat("HH:mm:ss")
            timeText.text = formatter.format( message.createdAt)
                    //DateUtils.formatDateTime(context, message.createdAt, DateUtils.FORMAT_SHOW_TIME)
        }
    }

    private inner class ReceivedMessageHolder internal constructor(itemView: View) : androidx.recyclerview.widget.RecyclerView.ViewHolder(itemView) {
        internal var messageText: TextView = itemView.findViewById(R.id.text_message_body) as TextView
        internal var timeText: TextView = itemView.findViewById(R.id.text_message_time) as TextView
        internal var nameText:TextView = itemView.findViewById(R.id.text_message_name) as TextView

        internal fun bind(message: Message) {
            messageText.text = message.text
            val formatter = SimpleDateFormat("HH:mm:ss")
            timeText.text = formatter.format( message.createdAt)
            nameText.text = message.sender
        }
    }

}