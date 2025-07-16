document.addEventListener('DOMContentLoaded', function () {
    const filterSpecies = document.getElementById('filterSpecies');
    const filterBreed = document.getElementById('filterBreed');
    const filterGender = document.getElementById('filterGender');
    const filterMinAge = document.getElementById('filterMinAge');
    const filterMaxAge = document.getElementById('filterMaxAge');
    const filterColor = document.getElementById('filterColor');
    const filterButton = document.getElementById('filterButton');
    const resetButton = document.getElementById('resetButton');
    const petCards = document.querySelectorAll('.pet-card');
    const noPetsMessage = document.querySelector('.no-pets-icon')?.parentElement;

    function filterPets() {
        let anyVisible = false;
        petCards.forEach(card => {
            const species = card.getAttribute('data-species')?.toLowerCase() || '';
            const breed = card.getAttribute('data-breed')?.toLowerCase() || '';
            const gender = card.getAttribute('data-gender') || '';
            const age = parseInt(card.getAttribute('data-age')) || 0;
            const color = card.getAttribute('data-color')?.toLowerCase() || '';

            let show = true;
            if (filterSpecies.value && !species.includes(filterSpecies.value.toLowerCase())) show = false;
            if (filterBreed.value && !breed.includes(filterBreed.value.toLowerCase())) show = false;
            if (filterGender.value && gender !== filterGender.value) show = false;
            if (filterMinAge.value && age < parseInt(filterMinAge.value)) show = false;
            if (filterMaxAge.value && age > parseInt(filterMaxAge.value)) show = false;
            if (filterColor.value && !color.includes(filterColor.value.toLowerCase())) show = false;

            if (show) {
                card.style.display = '';
                setTimeout(() => { card.style.opacity = 1; }, 10);
                anyVisible = true;
            } else {
                card.style.opacity = 0;
                setTimeout(() => { card.style.display = 'none'; }, 300);
            }
        });
        if (noPetsMessage) {
            noPetsMessage.style.display = anyVisible ? 'none' : '';
        }
    }

    function resetFilters() {
        filterSpecies.value = '';
        filterBreed.value = '';
        filterGender.value = '';
        filterMinAge.value = '';
        filterMaxAge.value = '';
        filterColor.value = '';
        filterPets();
    }

    filterButton.addEventListener('click', filterPets);
    resetButton.addEventListener('click', resetFilters);

    // Enter'a basınca da filtrele
    document.getElementById('petFilterForm').addEventListener('keydown', function(e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            filterPets();
        }
    });

    // Pet açıklamasını aç/kapa animasyonu
    document.querySelectorAll('.pet-description').forEach(desc => {
        desc.addEventListener('click', function() {
            this.classList.toggle('expanded');
        });
    });

    // Otomatik filtreleme için inputlara da event ekleyebilirsin:
    // [filterSpecies, filterBreed, filterGender, filterMinAge, filterMaxAge, filterColor].forEach(input => {
    //     input.addEventListener('input', filterPets);
    // });
}); 