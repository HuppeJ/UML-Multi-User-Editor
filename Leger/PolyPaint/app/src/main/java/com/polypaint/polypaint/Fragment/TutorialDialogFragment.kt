package com.polypaint.polypaint.Fragment

import android.app.Dialog
import android.content.DialogInterface
import android.graphics.Color
import android.graphics.Typeface
import android.os.Bundle
import android.view.View
import android.view.ViewGroup
import android.widget.ArrayAdapter
import android.widget.Button
import android.widget.LinearLayout
import android.widget.TextView
import androidx.appcompat.app.AlertDialog
import androidx.fragment.app.DialogFragment
import com.polypaint.polypaint.R
import com.polypaint.polypaint.Tutorial.Section
import com.polypaint.polypaint.Tutorial.Section1
import kotlinx.android.synthetic.main.dialog_tutorial.*
import kotlinx.android.synthetic.main.dialog_tutorial.view.*
import java.util.*
import kotlin.collections.ArrayList

class TutorialDialogFragment: DialogFragment() {

    var currentSection : Section? = null
    var indexes : ArrayList<String> = ArrayList()
    var buttons : ArrayList<Button> = ArrayList()
    var viewSelf : View? = null
    val lptitle : ViewGroup.LayoutParams = ViewGroup.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT,ViewGroup.LayoutParams.WRAP_CONTENT)
    var selectedIndex : Int = 0
    var selectedSectionIndex : Int = 0
    var mapSectionIndex : IntArray = intArrayOf(0,5,10,13)
    override fun onCreateDialog(savedInstanceState: Bundle?): Dialog {

        return activity?.let {
            val builder = AlertDialog.Builder(it)
            viewSelf = it.layoutInflater.inflate(R.layout.dialog_tutorial,null)

            val txtContainer : TextView = viewSelf!!.findViewById(R.id.text_container)
            buildIndexes()

            val btnSecNext: Button = viewSelf!!.findViewById(R.id.next_section)
            btnSecNext.setOnClickListener {
                selectedSectionIndex = (selectedSectionIndex+1) % mapSectionIndex.size
                onClickIndex(buttons[mapSectionIndex[selectedSectionIndex]])
            }

            val btnNext: Button = viewSelf!!.findViewById(R.id.next_subsection)
            btnNext.setOnClickListener {
                selectedIndex = (selectedIndex+1) % buttons.size
                onClickIndex(buttons[selectedIndex])
            }
            val btnPrev: Button = viewSelf!!.findViewById(R.id.previous_subsection)
            btnPrev.setOnClickListener {
                selectedIndex = if(selectedIndex-1 < 0) buttons.size - 1 else selectedIndex-1
                onClickIndex(buttons[selectedIndex])
            }

            val btnSecPrev: Button = viewSelf!!.findViewById(R.id.previous_section)
            btnSecPrev.setOnClickListener {
                selectedSectionIndex = if(selectedSectionIndex-1 < 0) mapSectionIndex.size - 1 else selectedSectionIndex-1
                onClickIndex(buttons[mapSectionIndex[selectedSectionIndex]])
            }

            onClickIndex(buttons[0])

            builder.setView(viewSelf)
                .setNegativeButton("Close",
                    DialogInterface.OnClickListener { dialog, id ->
                    })

            // Create the AlertDialog object and return it
            builder.create()
        } ?: throw IllegalStateException("Activity cannot be null")
    }

    override fun onDestroy() {
        closeModal()
        super.onDestroy()
    }
    private fun closeModal(){

    }
    private fun buildIndexes(){
        var titles = resources.getStringArray(R.array.tutorial_title_array).toList()
        var subtitles = resources.getStringArray(R.array.tutorial_sub_title_array).toList()
        var subindex : Int = 1
        var text : String = ""
        for( i in 0..titles.size-1){
            text = (i+1).toString() + " - "+ titles[i]
            indexes.add(text)
            buildButton(text,false)
            when(i){
                0->{
                    for(j in 0..3){
                        text = (i+1).toString() + "." + subindex.toString() + " - " +subtitles[j]
                        indexes.add(text)
                        buildButton(text,true)
                        subindex++
                    }
                }
                1->{
                    for(j in 4..7){
                        text = (i+1).toString() + "." + subindex.toString() + " - " +subtitles[j]
                        indexes.add(text)
                        buildButton(text,true)
                        subindex++
                    }
                }
                2->{
                    for(j in 8..9){
                        text = (i+1).toString() + "." + subindex.toString() + " - " +subtitles[j]
                        indexes.add(text)
                        buildButton(text,true)
                        subindex++
                    }
                }
                3->{
                    for(j in 10..subtitles.size-1){
                        text = (i+1).toString() + "." + subindex.toString() + " - " +subtitles[j]
                        indexes.add(text)
                        buildButton(text,true)
                        subindex++
                    }
                }
            }
            subindex = 1
        }
    }
    private fun buildButtons(){
        for (e in indexes){
            buildButton(e, false)
        }
    }
    private fun buildButton(text:String, isSubTitle:Boolean) : Button{
        var btn : Button = Button(context)
        btn.setText(text)
        btn.setTypeface(null, Typeface.NORMAL)
        btn.setBackgroundColor(Color.WHITE)
        if(isSubTitle){
            btn.setPadding(10,5,0,5)
        }else{
            btn.setPadding(0,5,0,5)
        }
        btn.gravity = 70
        lptitle.height = 50
        viewSelf!!.linearLayout5.addView(btn, lptitle)
        btn.setOnClickListener {
            onClickIndex(it)
        }
        buttons.add(btn)
        return btn

    }

    private fun onClickIndex(it:View){
        for(i in 0..buttons.size-1){
            buttons[i].setTypeface(null, Typeface.NORMAL)
            if(buttons[i] == it){
                selectedIndex = i
                buttons[i].setTypeface(null, Typeface.BOLD)
                (viewSelf!!.findViewById(R.id.text_container) as TextView).text = buttons[i].text
            }
        }
    }

}