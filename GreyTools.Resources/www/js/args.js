//args类
function Args(type,value){
    this.type = type;
    this.value = value;
}
function args_parse(str) {
    var value = "";//当前解析到的值
    var type = "";
    var lastMode = 0;
    var mode = 0;//当前模式,0普通模式,1类型模式,2文本模式,3转义模式
    var result=null;
    var added = 0;
    //i是当前的位置
    for (var i = 0; i < str.length; i++) {
        
        var text = str.slice(i, i + 1);//现在解析到的字符
        //如果转义的话,我们这里简单写一下(因为不会有什么意外情况)
        if (mode == 3) {
            mode = lastMode;
            if (text == "n") {
                value += "\n";
                continue;
            }
            value += text;
            continue;
        }
        if (text == "\"") {
            if (mode == 2) {
                mode = 0;
                continue;
            }
            mode = 2;
            continue;
        }
        if (text == "\\") {
            lastMode = mode;
            mode = 3;
            continue;
        }
        if (mode == 2) //文本模式中,下面的东西不会生效
        {
            value += text;
            continue;
        }
        if (text == "<") {
            mode = 1;
            continue;
        }
        if (text == ">") {
            mode = 0;
            //如果数组没有初始化,type值就是数组的长度
            if (result == null) {
                result = new Array();
                type = "";
            }
            continue;
        }
        if (text == ",")//这里我们添加它
        {
            result[added] = new Args(type, value);
            added++;
            value = "";
            type = "";
            continue;
        }
        //剩下的就是其他的字符了
        if (mode == 1) {
            type += text;
            continue;
        }
        else {
            value += text;
        }
    }
    return result;
}
function args_printInfo(arr) {
    for(var i=0;i<arr.length;i++){
        console.log("type:" + arr[i].type + ",value:" + arr[i].value);
    }
}
function args_escape(str) {
    return str.replace("\\", "\\\\").replace("\"", "\\\"");
}
function args_encodeArgs(arr) {
    if (arr == null)
    {
        return "<0>";
    }
    var temp = "<"+arr.length+">";
    for(var i=0;i<arr.length;i++){
        if (arr[i].type=="string") {
            temp += "<" + arr[i].type + ">\"" + args_escape(arr[i].value) + "\",";
            continue;
        }
        temp += "<" + arr[i].type + ">" + arr[i].value + ",";
    }
    return temp;
}