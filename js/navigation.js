function navigate() {
            var game = document.getElementById("game").value.toLowerCase();
            var type = document.getElementById("type").value.toLowerCase();
            var url = "https://amicitia.github.io/";

            if (game != "all" && type != "all")
                url = url + type + "s/" + game;
            else if (game == "all" && type != "all")
                url = url + type + "s";
            else if (type == "all" && game != "all")
                url = url + game;

            window.location.href = url;
        }