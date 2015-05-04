$(document).ready(function () {
    $("li.dropdown-submenu > a").on("click", function () {
        location.href = this.href;
    });
    $("li.dropdown-parentmenu > a").on("click", function () {
        location.href = this.href;
    });
});