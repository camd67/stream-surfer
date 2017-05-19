$(function () {
    $("#search-input").click(function () {
        $("#search-form").submit();
    });
    $("#global-search-form").hide(0);

    var suggestions = [
        "Game of Thrones",
        "Walking Dead",
        "Planet Earth",
        "Rick and Morty",
        "Parks and Rec",
        "Sherlock",
        "Archer",
        "Samurai Jack",
        "Attack on Titan",
        "Adventure Time",
        "Louie",
        "Grey's Anatomy",
        "Homeland",
        "Zero Punctuation"
    ];
    suggestions = shuffle(suggestions);
    var suggestionText = $(".home-suggestion-text");
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
        if (currentSuggestion === suggestions[suggestionIndex]) {
            deleting = true;
            toggleTypingInterval();
            return;
        }
        currentSuggestion += suggestions[suggestionIndex][currentSuggestion.length];
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