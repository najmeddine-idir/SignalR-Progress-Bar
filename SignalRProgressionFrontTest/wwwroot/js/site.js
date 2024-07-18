var connection = new signalR.HubConnectionBuilder().withUrl("http://localhost:5241/progressHub").build();

//Disable the send button until connection is established.
document.getElementById("sendButton").disabled = true;

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});
connection.on("ReceiveProgress", function (user, progress) {
    updateProgress(progress);
});

document.getElementById("sendButton").addEventListener("click", function (event) {

    fetch("http://localhost:5241/progress", { method: "Post" })
    .catch(console.log)

    event.preventDefault();
});

function updateProgress({ progressPercentage, remainingTime }) {
    $(".time").text(remainingTime);
    $(".progress-container .percentage").text(progressPercentage);
    $(".progress-container .percentage").css("left", progressPercentage);
    $(".progress-container .progress").width(progressPercentage);
}