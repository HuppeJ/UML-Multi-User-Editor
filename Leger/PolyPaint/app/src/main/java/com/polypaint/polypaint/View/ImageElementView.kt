package com.polypaint.polypaint.View

import androidx.appcompat.app.AppCompatActivity
import android.content.Context
import android.graphics.Color
import android.graphics.ColorFilter
import android.graphics.PorterDuff
import android.os.Bundle
import android.util.Log
import android.view.MotionEvent
import android.view.View
import android.widget.ImageView
import android.widget.RelativeLayout
import android.widget.TextView
import androidx.fragment.app.DialogFragment
import com.polypaint.polypaint.Enum.ShapeTypes
import com.polypaint.polypaint.Fragment.EditClassDialogFragment
import com.polypaint.polypaint.Holder.ViewShapeHolder
import com.polypaint.polypaint.R
import kotlinx.android.synthetic.main.basic_element.view.*
import kotlinx.android.synthetic.main.view_image_element.view.*


class ImageElementView(context: Context, shapeType: ShapeTypes): BasicElementView(context) {
    override var mMinimumWidth : Float = 220F
    override var mMinimumHeight : Float = 320F
    private var shapeType : ShapeTypes? = shapeType
    private var imgBackground : ImageView? = null

    override fun onAttachedToWindow() {
        super.onAttachedToWindow()

        val activity : AppCompatActivity = context as AppCompatActivity

        var child: View = activity.layoutInflater.inflate(R.layout.view_image_element, null)

        var nameText: TextView = child.findViewById(R.id.name) as TextView
        // TODO : Initialiser le text avec le basictElement.name lorsqu'on aura déterminé comment les view vont être parsées
        nameText.text = "basictElement.name"

        /*
        val shape = ViewShapeHolder.getInstance().canevas.findShape(
            ViewShapeHolder.getInstance().map.getValue(this)
        )
        Log.d("*******", shape.toString())
        */

        var img: ImageView = child.findViewById(R.id.image_element) as ImageView
        imgBackground = img
        when(this.shapeType){
            ShapeTypes.DEFAULT->{ }

            ShapeTypes.ARTIFACT -> {
                img.setBackgroundResource(R.drawable.ic_artifact)
            }
            ShapeTypes.ACTIVITY -> {
                img.setBackgroundResource(R.drawable.ic_activity)


            }
            ShapeTypes.ROLE -> {
                img.setBackgroundResource(R.drawable.ic_actor)

            }

        }

        borderResizableLayout.addView(child)


        // TODO : On ne peut pas mettre à jour les dimmenssion d'une vue dans onAttachedToWindow alors le code ci-dessous ne fait rien en ce moment
        // TODO : Pour le moment je n'ai pas trouvé d'alternative qui fonctionne sans interférer avec le resize.
        borderResizableLayout.layoutParams.width =  (mMinimumWidth).toInt()

        borderResizableLayout.layoutParams.height = (mMinimumHeight).toInt()
        linearLayoutCompat.layoutParams.height = (9*mMinimumHeight/10).toInt()
        linearLayoutCompat2.layoutParams.height = (1*mMinimumHeight/10).toInt()

    }

    override fun resize(newWidth:Int, newHeight:Int){
        if(newWidth >= mMinimumWidth){
            borderResizableLayout.layoutParams.width = newWidth
        }

        if(newHeight >= mMinimumHeight){
            borderResizableLayout.layoutParams.height = newHeight
            linearLayoutCompat.layoutParams.height = (9*newHeight / 10)
            linearLayoutCompat2.layoutParams.height = (1 * newHeight / 10)
        }

        borderResizableLayout.requestLayout()
        requestLayout()
    }

    override fun outlineColor(color : String){
        when(color){
            "BLACK" -> {imgBackground!!.background.mutate().setColorFilter(Color.BLACK, PorterDuff.Mode.SRC_IN)}
            "GREEN" -> {imgBackground!!.background.mutate().setColorFilter(Color.GREEN, PorterDuff.Mode.SRC_IN)}
            "YELLOW" -> {imgBackground!!.background.mutate().setColorFilter(Color.YELLOW, PorterDuff.Mode.SRC_IN)}
        }

    }
}