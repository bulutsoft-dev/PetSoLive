function filterLostPetCards() {
    var search = document.getElementById('searchInput').value.toLowerCase();
    var city = document.getElementById('cityFilterBar').value.toLowerCase();
    var cards = document.querySelectorAll('.lostpet-card');
    cards.forEach(function(card) {
        var cardName = (card.getAttribute('data-name') || '').toLowerCase();
        var cardDesc = (card.getAttribute('data-description') || '').toLowerCase();
        var cardCity = (card.getAttribute('data-city') || '').toLowerCase();
        var show = true;
        if (city && cardCity !== city) show = false;
        if (search && !(cardName.includes(search) || cardDesc.includes(search))) show = false;
        card.style.display = show ? '' : 'none';
    });
}

document.addEventListener('DOMContentLoaded', function() {
    var searchInput = document.getElementById('searchInput');
    var cityFilterBar = document.getElementById('cityFilterBar');
    if (searchInput) searchInput.addEventListener('input', filterLostPetCards);
    if (cityFilterBar) cityFilterBar.addEventListener('change', filterLostPetCards);
    // Temizle butonu
    var clearBtn = document.getElementById('clearSearch');
    if (clearBtn) {
        clearBtn.addEventListener('click', function(e) {
            e.preventDefault();
            if (searchInput) searchInput.value = '';
            if (cityFilterBar) cityFilterBar.value = '';
            filterLostPetCards();
        });
    }
    filterLostPetCards();
}); 