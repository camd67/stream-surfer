$(function () {
    $("#search-input").click(function () {
        $("#search-form").submit();
    });
    $("#global-search-form").hide(0);
    $("#search-form").submit(function () {
        $(".loading-overlay").fadeIn(500);
    });
    var suggestions = [
        {
            display: "Game of Thrones",
            image: "got.jpg"
        },
        {
            display: "Walking Dead",
            image: "walkingdead.jpg"
        },
        {
            display: "Planet Earth",
            image: "pe.jpg"
        },
        {
            display: "Rick and Morty",
            image: "rick_and_morty.jpg"
        },
        {
            display: "Parks and Rec",
            image: "parks_and_rec.jpg"
        },
        {
            display: "Sherlock",
            image: "sherlock.jpg"
        },
        {
            display: "Archer",
            image: "archer.jpg"
        },
        {
            display: "Samurai Jack",
            image: "samuraijack.jpg"
        },
        {
            display: "Attack on Titan",
            image: "aot.jpg"
        },
        {
            display: "Adventure Time",
            image: "adventuretime.jpg"
        },
        {
            display: "Big Hero 6",
            image: "bh6.jpg"
        }/*,
        {
            display: "Louie",
            image: "louie.jpg"
        },
        {
            display: "Grey's Anatomy",
            image: "greysanatomy.jpg"
        },
        {
            display: "Homeland",
            image: "homeland.jpg"
        },
        {
            display: "Zero Punctuation",
            image: "zeropunc.jpg"
        }
        */
    ];
    suggestions = shuffle(suggestions);
    var suggestionText = $(".home-suggestion-text");
    var header = $(".header-image");
    var suggestionDelay = 3000;
    var typingSpeed = 100;
    var currentSuggestion = "";
    var suggestionIndex = 0;
    var deleting = false;
    toggleTypingInterval();
    
    var typingIntervalId = null;
    function animateDeleting() {
        if (currentSuggestion.length <= 0) {
            deleting = false;
            currentSuggestion = "";
            suggestionIndex = (suggestionIndex + 1) % suggestions.length;
            toggleTypingInterval();
            return;
        }
        currentSuggestion = currentSuggestion.substr(0, currentSuggestion.length - 1);
        suggestionText.text(currentSuggestion);
    }
    function animateTyping() {
        if (currentSuggestion === suggestions[suggestionIndex].display) {
            deleting = true;
            toggleTypingInterval();
            return;
        }
        currentSuggestion += suggestions[suggestionIndex].display[currentSuggestion.length];
        suggestionText.text(currentSuggestion);
    }
    function toggleTypingInterval() {
        if (deleting) {
            if (typingIntervalId) {
                clearInterval(typingIntervalId);
            }
            setTimeout(function () {
                typingIntervalId = setInterval(animateDeleting, typingSpeed);
            }, suggestionDelay);
        } else {
            if (typingIntervalId) {
                clearInterval(typingIntervalId);
            }
            var aniDelay = 500;
            header.fadeOut(aniDelay, function () {
                    header.css("background-image", "url(\"../images/homepage/" + suggestions[suggestionIndex].image + "\")");
                    header.fadeIn(aniDelay);
                });
            setTimeout(function () {
                typingIntervalId = setInterval(animateTyping, typingSpeed);
            }, suggestionDelay / 4);
        }
    }

    function shuffle(array) {
        var currentIndex = array.length
        var temp;
        var randomIndex;
        while (0 !== currentIndex) {
            randomIndex = Math.floor(Math.random() * currentIndex);
            currentIndex -= 1;
            temp = array[currentIndex];
            array[currentIndex] = array[randomIndex];
            array[randomIndex] = temp;
        }
        return array;
    }

});