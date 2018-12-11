var factions = [];
var factionFilter = document.forms['deck-filters']['factionFilter'];

if (factionFilter.value !== '') {
    factions = JSON.parse(factionFilter.value);
    for (var i = 0; i < factions.length; i++) {
        $('#' + factions[i]).attr('src', 'images/icons/factions/' + factions[i].toLowerCase() + '-icon-select.png');
        $('#' + factions[i]).attr('id', factions[i] + '-Select');
    }
}

// Highlight Faction Filters

$('.faction-icon')
    .mouseenter(function () {
        switch (this.id) {
            case 'Fire':
                $(this).attr('src', 'images/icons/factions/fire-icon-select.png');
                break;
            case 'Time':
                $(this).attr('src', 'images/icons/factions/time-icon-select.png');
                break;
            case 'Justice':
                $(this).attr('src', 'images/icons/factions/justice-icon-select.png');
                break;
            case 'Primal':
                $(this).attr('src', 'images/icons/factions/primal-icon-select.png');
                break;
            case 'Shadow':
                $(this).attr('src', 'images/icons/factions/shadow-icon-select.png');
                break;
        }
    })
    .mouseleave(function () {
        switch (this.id) {
            case 'Fire':
                $(this).attr('src', 'images/icons/factions/fire-icon.png');
                break;
            case 'Time':
                $(this).attr('src', 'images/icons/factions/time-icon.png');
                break;
            case 'Justice':
                $(this).attr('src', 'images/icons/factions/justice-icon.png');
                break;
            case 'Primal':
                $(this).attr('src', 'images/icons/factions/primal-icon.png');
                break;
            case 'Shadow':
                $(this).attr('src', 'images/icons/factions/shadow-icon.png');
                break;
        }
    })
    .click(function () {
        switch (this.id) {
            case 'Fire':
                factions.push(this.id);
                $(this).attr('src', 'images/icons/factions/fire-icon-select.png');
                $(this).attr('id', 'Fire-Select');
                break;
            case 'Fire-Select':
                $(this).attr('src', 'images/icons/factions/fire-icon.png');
                $(this).attr('id', 'Fire');
                var deselectedFaction = this.id;
                factions = factions.filter(function (faction) {
                    return faction !== deselectedFaction;
                });
                break;
            case 'Time':
                factions.push(this.id);
                $(this).attr('src', 'images/icons/factions/time-icon-select.png');
                $(this).attr('id', 'Time-Select');
                break;
            case 'Time-Select':
                $(this).attr('src', 'images/icons/factions/time-icon.png');
                $(this).attr('id', 'Time');
                var deselectedFaction = this.id;
                factions = factions.filter(function (faction) {
                    return faction !== deselectedFaction;
                });
                break;
            case 'Justice':
                factions.push(this.id);
                $(this).attr('src', 'images/icons/factions/justice-icon-select.png');
                $(this).attr('id', 'Justice-Select');
                break;
            case 'Justice-Select':
                $(this).attr('src', 'images/icons/factions/justice-icon.png');
                $(this).attr('id', 'Justice');
                var deselectedFaction = this.id;
                factions = factions.filter(function (faction) {
                    return faction !== deselectedFaction;
                });
                break;
            case 'Primal':
                factions.push(this.id);
                $(this).attr('src', 'images/icons/factions/primal-icon-select.png');
                $(this).attr('id', 'Primal-Select');
                break;
            case 'Primal-Select':
                $(this).attr('src', 'images/icons/factions/primal-icon.png');
                $(this).attr('id', 'Primal');
                var deselectedFaction = this.id;
                factions = factions.filter(function (faction) {
                    return faction !== deselectedFaction;
                });
                break;
            case 'Shadow':
                factions.push(this.id);
                $(this).attr('src', 'images/icons/factions/shadow-icon-select.png');
                $(this).attr('id', 'Shadow-Select');
                break;
            case 'Shadow-Select':
                $(this).attr('src', 'images/icons/factions/shadow-icon.png');
                $(this).attr('id', 'Shadow');
                var deselectedFaction = this.id;
                factions = factions.filter(function (faction) {
                    return faction !== deselectedFaction;
                });
                break;
        }
    });

// Reset Filters

$('#reset-filter-link').click(function (e) {
    e.preventDefault();
    document.forms['deck-filters']['searchFilter'].value = '';
    document.forms['deck-filters']['factionFilter'].value = '';
    document.forms['deck-filters']['userFilter'].value = '';
    for (var i = 0; i < factions.length; i++) {
        $('#' + factions[i] + '-Select')
            .attr('src', 'images/icons/factions/' + factions[i].toLowerCase() + '-icon.png')
            .attr('id', factions[i]);
    }
    factions = [];
    console.log(factions);
});

// Submit Filters Form

$('#deck-filters').submit(function () {
    var orderedFactions = [];
    var factionList = ['Fire', 'Time', 'Justice', 'Primal', 'Shadow'];
    factionList.forEach(function (faction) {
        if (factions.includes(faction)) {
            orderedFactions.push(faction);
        }
    });

    if (orderedFactions.length === 0) {
        factionFilter.value = '';
    } else {
        factionFilter.value = JSON.stringify(orderedFactions);
    }
});