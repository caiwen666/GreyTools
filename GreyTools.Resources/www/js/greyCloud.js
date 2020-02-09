//greyTools的基本库

//全局声明
activeList = null;//当前选中的列表的class
tools_map = null;
tools_map = new Map();

///发送控制台调试信息（debug）
///level：消息等级，1信息，2注意，3错误
///org：来源
///text：消息内容
function log(level,org,text){
	var a="";
	if(level==1){
		a="[信息]";
	}
	if(level==2){
		a="[注意]";
	}
	if(level==3){
		a="[错误]";
	}else{
		a="[未知]";
	}
	console.log(a+"["+org+"]"+text);
}

///切换页面
///name：页面的名称
///tname：页面在page文件夹中的文件名，不带后缀
function setPage(name,tname){
	var htmlobj=$.ajax({url:"page/"+tname+".html",async:false});
	$("#main").html(htmlobj.responseText);
	$("#title").text(name);
}

//设置选中
function setClick(id) {
    if (activeList != null) {//撤销之前的选中
        $("." + activeList).removeClass("mdui-list-item-active");
        $("." + activeList + "_text").removeClass("mdui-text-color-theme");
    }
    //应用现在的选中
    activeList = id;
    $("." + activeList).addClass("mdui-list-item-active");
    $("." + activeList + "_text").addClass("mdui-text-color-theme");
    $("#main").css("left", "-100px");//动画准备
    $("#main").css("opacity", "0");
}

///设置点击事件(private)
///id，目标的名称
function button_gt(id) {
    setClick("button_" + id)
	setPage($("."+activeList+"_text").text(),id);//设置
	//执行界面附带的脚本
	$.getScript("page/"+id+".js");
	//执行动画
	setTimeout(function () {
		$("#main").animate({
		    left:'250px',
		    opacity:'1'
		});
	}, 500);
	mdui.mutation();
}

///初始化
button_gt("home");
//layer按钮的设置
var inst1 = new mdui.Menu('#layer', '#layer_menu');
//layer按钮
document.getElementById('layer').addEventListener('click', function () {
    inst1.open();
    $("#layer_menu").width("180px");
});

//运行工具
function runTools(name) {
    //设置选中
    setClick("button_tools_" + name);
    //运行
    if (ToolsManager.runTools(name)) {
        //返回true就说明已经在后端加载完毕,我们在前端直接调用即可
        //我们加个map做缓存,如果map中已经有tools的对象了,我们就不加载它,反之就加载
        if (!tools_map.has(name)) {
            $.getScript('https://' + name + '.greycloud.com/main/main.js', function () {
                tools_map.set(name, new toolsMain());
            });
        } 
        //然后加载界面
        var htmlobj = $.ajax({ url: 'https://' + name + '.greycloud.com/main/main.html', async: false });
        $("#main").html(htmlobj.responseText);
        tools_map.get(name).Load();
        $("#title").text(tools_map.get(name).name);
        //然后是动画
        setTimeout(function () {
            $("#main").animate({
                left: '250px',
                opacity: '1'
            });
        }, 500);
        mdui.mutation();
    }
}

