// add regex test to jquery validate
$.validator.addMethod(
    "teamurlRegex",
    function (value, element, regexp) {
        var re = new RegExp(regexp, "i");
        return this.optional(element) || re.test(value);
    },
    "Please enter a valid Team URL"
);

// Cached Data for teams that are displayed
var rawData = [];
var filterData = [];
var chartData = {
    bans: [],
    picks: [],
    disregarded: [],
    played: [],
    mapsWon: [],
    mapsLost: [],
    roundsWon: [],
    roundsLost: []
};
var charts = {};

/**
 * Add a new tab if necessary and select it
 */
function addTab(cacheIndex) {
    if (rawData.length < 1)
        return false;

    if (cacheIndex == -1)
        cacheIndex = rawData.length - 1;

    let tabId = cacheIndex + 1;
    let team = rawData[cacheIndex].team;
    let $teamTabs = $('#team-tabs');
    let $teamTabPanes = $('#team-tabpanes');
    let $elTab;

    if ($('#team-tab-' + tabId).length < 1) {
        $elTab = $('<a class="nav-item nav-link" data-toggle="tab" role="tab" href="#team" id="team-tab-' + tabId + '">').text(team.name);
        if (!!team.logoURL) {
            $elTab.prepend('<img src="' + team.logoURL + '" class="mr-3" style="width: 35px;">');
        }

        $elTab.appendTo($teamTabs);
    }
    else {
        $elTab = $('#team-tab-' + tabId);
    }

    if (!$teamTabs.parent().hasClass("show")) {
        $teamTabs.parent().addClass("show");
    }
    if (!$teamTabPanes.hasClass("show")) {
        $teamTabPanes.addClass("show");
    }

    $elTab.tab('show');
}

/**
 * Cache management: Add new data or overwrite old data
 */
function addToCache(responseData) {
    var cacheIndex = -1;
    $.each(rawData, function (i, v) {
        if (v.team.url == responseData.team.url)
            cacheIndex = i;
    });

    if (cacheIndex >= 0) {
        rawData[cacheIndex] = {
            team: responseData.team,
            matches: responseData.matches
        };
    }
    else {
        rawData.push({
            team: responseData.team,
            matches: responseData.matches
        });
    }

    return cacheIndex;
}

/**
 * Interpret Data according to filters
 * (TODO)
 */
function interpretData(cacheIndex) {
    if (rawData.length < 1)
        return false;

    if (cacheIndex == -1)
        cacheIndex = rawData.length - 1;

    let data = rawData[cacheIndex],
        bans = [], picks = [],
        enemyBans = [], enemyPicks = [],
        disregarded = [], played = [],
        mapsWon = [], mapsLost = [],
        roundsWon = {}, roundsLost = {};

    // we're tracking all vote data we have - ignoring if the match was actually played or not
    $.each(data.matches.filter(m => m.votes.length > 0), function (i, match) {
        // our teamnumber - not making sure if we're even in the names object.
        // trusts the backend's data integrity..
        let teamNumber = match.teamNames.A == data.team.name ? 0 : 1;
        let enemyTeamNumber = match.teamNames.A == data.team.name ? 1 : 0;

        $.each(match.votes, function (i, vote) {
            if (vote.type == 0) {
                // ban
                if (vote.team == teamNumber)
                    bans.push(vote.map);
                else
                    enemyBans.push(vote.map);
            }
            else if (vote.type == 1) {
                // pick
                if (vote.team == teamNumber)
                    picks.push(vote.map);
                else
                    enemyPicks.push(vote.map);
            }
        });

        // get disregarded maps depending on the mappool
        // once again, trusting the backend's data integrity..
        let votedMaps = match.votes.map(v => v.map.name);
        let seasons = Object.keys(_MapPool).map(i => parseInt(i));
        let appliedMapPool = seasons.filter(n => n <= match.season.number);
        appliedMapPool = appliedMapPool[appliedMapPool.length - 1];
        let disregardedMaps = _MapPool[appliedMapPool].filter(m => votedMaps.indexOf(m) === -1);

        disregarded = disregarded.concat(disregardedMaps);

        // get map losses / wins
        for (var map of match.maps) {
            let mapScore = Object.values(map.score);

            // maps won/lost
            if (mapScore[teamNumber] > mapScore[enemyTeamNumber])
                mapsWon.push(map.name);
            else
                mapsLost.push(map.name);

            // rounds won
            if (Object.keys(roundsWon).indexOf(map.name) === -1)
                roundsWon[map.name] = mapScore[teamNumber];
            else
                roundsWon[map.name] += mapScore[teamNumber]

            // rounds lost
            if (Object.keys(roundsLost).indexOf(map.name) === -1)
                roundsLost[map.name] = mapScore[enemyTeamNumber];
            else
                roundsLost[map.name] += mapScore[enemyTeamNumber];
        }
    });

    let tmpPlayed = data.matches.filter(m => m.status == 2).map(m => m.maps);
    for (var maps of tmpPlayed) {
        for (var map of maps) {
            played.push(map.name);
        }
    }

    // set data
    chartData.bans[cacheIndex] = bans.map(m => m.name).reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
    chartData.picks[cacheIndex] = picks.map(m => m.name).reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
    chartData.disregarded[cacheIndex] = disregarded.reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
    chartData.played[cacheIndex] = played.reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
    chartData.mapsWon[cacheIndex] = mapsWon.reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
    chartData.mapsLost[cacheIndex] = mapsLost.reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
    chartData.roundsWon[cacheIndex] = roundsWon;
    chartData.roundsLost[cacheIndex] = roundsLost;
}

/**
 * Creates all map charts from the chartData object
 */
function createCharts(cacheIndex) {
    if (cacheIndex == -1)
        cacheIndex = rawData.length - 1;

    // (re)initialize charts
    charts.bans = createMapChart('bans-chart', chartData.bans[cacheIndex]);
    charts.picks = createMapChart('picks-chart', chartData.picks[cacheIndex]);
    charts.disregarded = createMapChart('disregarded-chart', chartData.disregarded[cacheIndex]);
    charts.played = createMapChart('played-chart', chartData.played[cacheIndex]);
    charts.mapsWon = createMapChart('maps-won-chart', chartData.mapsWon[cacheIndex]);
    charts.mapsLost = createMapChart('maps-lost-chart', chartData.mapsLost[cacheIndex]);
    charts.roundsWon = createMapChart('rounds-won-chart', chartData.roundsWon[cacheIndex]);
    charts.roundsLost = createMapChart('rounds-lost-chart', chartData.roundsLost[cacheIndex]);
}

/**
 * Creates a map chart and returns its handle
 * Expands the label array with the current mappool
 */
function createMapChart(element, mapData) {
    let currentMapPool = Object.values(_MapPool);
    currentMapPool = currentMapPool[currentMapPool.length - 1];

    let mapLabels = Object.keys(mapData || {});
    for (let map of currentMapPool) {
        if (mapLabels.indexOf(map) === -1)
            mapLabels.push(map);
    }

    ctx = document.getElementById(element);
    return new Chart(ctx, {
        type: 'pie',
        data: {
            labels: mapLabels,
            datasets: [{
                data: Object.values(mapData || {}),
                backgroundColor: ["#007bff", "#28a745", "#dc3545", "#ffc107", "#9C27B0", "#FF9800", "#03f2f4", "#795548", "#9e9e9e"],
            }]
        },
        options: {
            maintainAspectRatio: false,
        }
    });
}

/**
 * (Re-)Bind card collapse button events
 */
function bindCollapseEvents() {
    $('button.collapse-toggle').off('click');
    $('button.collapse-toggle').on('click', function () {
        let $this = $(this),
            $target = $($this.data('target')),
            svgMaximize = feather.icons["maximize-2"],
            svgMinimize = feather.icons["minimize-2"],
            chosenSvg;

        if ($target.hasClass("collapsing"))
            return false;

        if ($target.hasClass("show"))
            chosenSvg = svgMaximize;
        else
            chosenSvg = svgMinimize;

        $this.find('svg').attr('class', chosenSvg.attrs.class).html(chosenSvg.contents);
    });
}