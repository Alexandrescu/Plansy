function onAddMessageSuccess(data) {
    if (data) {
        var messageId = data.messageId;
        var content = "<div id=\"MessageItem-" + messageId + "\" class=\"message-box\">";
        content += data.view;
        content += "</div>";

        $("#Messages").append(content);
    }
}