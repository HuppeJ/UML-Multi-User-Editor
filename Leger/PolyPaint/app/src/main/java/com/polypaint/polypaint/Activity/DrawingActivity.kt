package com.polypaint.polypaint.Activity

import android.content.Intent
import android.content.pm.ActivityInfo
import android.graphics.Bitmap
import android.graphics.Canvas
import android.graphics.Paint
import android.os.Bundle
import android.view.MotionEvent
import android.view.View
import android.widget.Button
import android.widget.ImageView
import androidx.appcompat.app.AppCompatActivity
import com.github.nkzawa.socketio.client.Socket
import com.polypaint.polypaint.Application.PolyPaint
import com.polypaint.polypaint.Model.BasicShape
import com.polypaint.polypaint.R

class DrawingActivity : AppCompatActivity(){

    private var mImageView : ImageView? = null
    private var mPaint : Paint? = null
    private var mCanvas : Canvas? = null
    private var offset : Float = 50f
    private var count : Int = 0

    override fun onCreate (savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        requestedOrientation = ActivityInfo.SCREEN_ORIENTATION_LANDSCAPE
        setContentView(R.layout.activity_drawing)
        mImageView = findViewById(R.id.canvasHolder)

        var canvasButton: Button = findViewById(R.id.canvas_button)
        canvasButton.setOnClickListener {
            initCanvas()
        }
        var drawButton: Button = findViewById(R.id.draw_button)
        drawButton.setOnClickListener {
            drawSomething()
        }
    }

    private fun initCanvas(){
        val mBitmap = Bitmap.createBitmap(mImageView!!.width,mImageView!!.height, Bitmap.Config.ARGB_8888);
        mPaint = Paint()
        mCanvas = Canvas(mBitmap)
        mImageView!!.setImageBitmap(mBitmap)
    }

    private fun drawSomething(){
        mCanvas!!.drawColor( getResources().getColor(R.color.colorPrimary))
        mCanvas!!.drawText("DU TEXTE", 100F, 100F+(count*offset), mPaint)
        count ++
    }

    override fun onBackPressed() {
        val app = application as PolyPaint
        val socket: Socket? = app.socket
        socket?.disconnect()
        val intent = Intent(this, LoginActivity::class.java)
        intent.flags = Intent.FLAG_ACTIVITY_CLEAR_TOP
        startActivity(intent)
        finish()
    }
}