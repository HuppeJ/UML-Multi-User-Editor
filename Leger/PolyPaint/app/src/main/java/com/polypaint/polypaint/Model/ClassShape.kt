package com.polypaint.polypaint.Model

class ClassShape (id: String, type: Int, name: String, shapeStyle: ShapeStyle, links: Array<String?>, var attributes: ArrayList<String?>, var methods: ArrayList<String?>):BasicShape(id, type, name, shapeStyle, links){}