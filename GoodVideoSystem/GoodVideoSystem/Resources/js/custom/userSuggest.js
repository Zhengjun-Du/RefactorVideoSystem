//用户意见反馈

function trim(t) {
    return (t||"").replace(/^\s+|\s+$/g, "");
}

function submitSuggest() {
    var suggestText = $("#suggestText").val();
    suggestText = trim(suggestText);
    if (suggestText.length <= 0) {
        jQuery("#info").text("反馈内容不能为空！");
        return;
    }

    var phone = $("#phone").val();
    phone = trim(phone);
    if (phone.length <= 0)
    {
        jQuery("#info").text("手机号不能为空！");
        return;
    }

    var myreg = /^(((13[0-9]{1})|(15[0-9]{1})|(18[0-9]{1}))+\d{8})$/;
    if (!myreg.test(phone)) {
        alert('请输入有效的手机号码！');
        return;
    }

    jQuery.post(
        "/User/Suggest",
        {
            "suggestText": suggestText,
            "phone": phone
        },
        function (data) {
            if (data == "success") {
                alert("您的留言发送成功！");
                $("#suggestText").val("");
                $("#phone") .val("");
            } else {
                alert("您的留言发送失败！");
            }
        }
    );
}

//获得焦点清除提示信息
function clearInfo() {
    jQuery("#info").text("");
}