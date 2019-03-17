package com.polypaint.polypaint.Particles

import android.content.Context
import android.graphics.*
import kotlin.math.max


class BasicParticle (xOrigin: Float, yOrigin:Float, var maxSpeed :Float, var context: Context) {
    val ALIVE = 0
    val DEAD = 1
    private var mState: Int = ALIVE
    //private var mBitmap: Bitmap? = null
    private var mAge: Int = 0
    private var mLifetime: Int = 25
    private var mPaint: Paint = Paint()
    //private var mBase: Bitmap? = null
    var x : Float = 0F
    var y : Float = 0F
    var vX : Float = 0F
    var vY : Float = 0F
    var aX : Float = 0F
    var aY : Float = 0F
    init {
        //Make Bitmap based on drawable
        //mBase = BitmapFactory.decodeResource(context.resources, R.drawable.ic_plus)
        //set random scale
        //Make new bitmap with new width and height
        //mBitmap = Bitmap.createScaledBitmap(mBase,100,100,true)
        //mBitmap = mBase
        //Set Random direction
        this.x = xOrigin
        this.y = yOrigin
        vX = maxSpeed * initDirection()
        vY = maxSpeed * initDirection()

        //Circle splat
        if(vX*vX + vY*vY >= maxSpeed * maxSpeed){
            vX *= 0.7F
            vY *= 0.7F
        }


    }
    //Splatter Effect
    fun initDirection (): Float{
        return ((Math.random()*2)-1).toFloat()
    }

    fun update(){
        if(mState != DEAD){
            //Mouvement Euler Simple
            this.x += this.vX
            this.y += this.vY
            if(vX*vX + vY*vY <= maxSpeed * maxSpeed){
                this.vX += this.aX
                this.vY += this.aY
            }
            mAge ++
            if(mAge >= mLifetime){
               mState = DEAD
            }

        }
    }

    fun isAlive():Boolean{
        return mState == ALIVE
    }

    fun draw(canvas: Canvas){
        canvas.drawCircle(this.x,this.y,5.0f, mPaint)
    }
}