//args��
function Args(type,value){
    this.type = type;
    this.value = value;
}
function args_parse(str) {
    var value = "";//��ǰ��������ֵ
    var type = "";
    var lastMode = 0;
    var mode = 0;//��ǰģʽ,0��ͨģʽ,1����ģʽ,2�ı�ģʽ,3ת��ģʽ
    var result=null;
    var added = 0;
    //i�ǵ�ǰ��λ��
    for (var i = 0; i < str.length; i++) {
        
        var text = str.slice(i, i + 1);//���ڽ��������ַ�
        //���ת��Ļ�,���������дһ��(��Ϊ������ʲô�������)
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
        if (mode == 2) //�ı�ģʽ��,����Ķ���������Ч
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
            //�������û�г�ʼ��,typeֵ��������ĳ���
            if (result == null) {
                result = new Array();
                type = "";
            }
            continue;
        }
        if (text == ",")//�������������
        {
            result[added] = new Args(type, value);
            added++;
            value = "";
            type = "";
            continue;
        }
        //ʣ�µľ����������ַ���
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