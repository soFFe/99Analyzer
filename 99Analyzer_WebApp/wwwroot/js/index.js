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
var chartData = {
    bans: [],
    picks: [],
    disregarded: []
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
        disregarded = [];

    // we're tracking all vote data we have - ignoring if the match was actually played or not
    $.each(data.matches.filter(m => m.votes.length > 0), function (i, match) {
        // our teamnumber - not making sure if we're even in the names object.
        // trusts the backend's data integrity..
        let teamNumber = match.teamNames.A == data.team.name ? 0 : 1;

        $.each(match.votes, function (i, vote) {
            if (vote.type == 0) {
                if (vote.team == teamNumber)
                    bans.push(vote.map);
                else
                    enemyBans.push(vote.map);
            }
            else if (vote.type == 1) {
                if(vote.team == teamNumber)
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
    });

    chartData.bans[cacheIndex] = bans.map(m => m.name).reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
    chartData.picks[cacheIndex] = picks.map(m => m.name).reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
    chartData.disregarded[cacheIndex] = disregarded.reduce((a, c) => (a[c] = (a[c] || 0) + 1, a), Object.create(null));
}

/**
 * Create the charts from the chartsData object
 * Considers the mappool
 */
function createCharts(cacheIndex) {
    if (cacheIndex == -1)
        cacheIndex = rawData.length - 1;

    let currentMapPool = Object.values(_MapPool);
    currentMapPool = currentMapPool[currentMapPool.length - 1];

    // bans chart
    let banLabels = Object.keys(chartData.bans[cacheIndex] || {});
    for (let map of currentMapPool) {
        if (banLabels.indexOf(map) === -1)
            banLabels.push(map);
    }
    let ctx = document.getElementById('bans-chart');
    charts.bans = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: banLabels,
            datasets: [{
                data: Object.values(chartData.bans[cacheIndex] || {}),
                backgroundColor: ["#007bff", "#28a745", "#dc3545", "#ffc107", "#9C27B0", "#FF9800", "#03f2f4", "#795548", "#9e9e9e"],
            }]
        },
        options: {
            maintainAspectRatio: false,
        }
    });

    // picks chart
    let pickLabels = Object.keys(chartData.picks[cacheIndex] || {});
    for (let map of currentMapPool) {
        if (pickLabels.indexOf(map) === -1)
            pickLabels.push(map);
    }
    ctx = document.getElementById('picks-chart');
    charts.picks = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: pickLabels,
            datasets: [{
                data: Object.values(chartData.picks[cacheIndex] || {}),
                backgroundColor: ["#007bff", "#28a745", "#dc3545", "#ffc107", "#9C27B0", "#FF9800", "#03f2f4", "#795548", "#9e9e9e"],
            }]
        },
        options: {
            maintainAspectRatio: false,
        }
    });

    // disregarded chart
    let drLabels = Object.keys(chartData.picks[cacheIndex] || {});
    for (let map of currentMapPool) {
        if (drLabels.indexOf(map) === -1)
            drLabels.push(map);
    }
    ctx = document.getElementById('disregarded-chart');
    charts.picks = new Chart(ctx, {
        type: 'pie',
        data: {
            labels: drLabels,
            datasets: [{
                data: Object.values(chartData.disregarded[cacheIndex] || {}),
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
    $('button.collapse-toggle').on('click', function(){
        let $this = $(this),
            $target = $($this.data('target')),
            svgMaximize = feather.icons["maximize-2"],
            svgMinimize = feather.icons["minimize-2"],
            chosenSvg;

        if($target.hasClass("collapsing"))
            return false;

        if($target.hasClass("show"))
            chosenSvg = svgMaximize;
        else
            chosenSvg = svgMinimize;

        $this.find('svg').attr('class', chosenSvg.attrs.class).html(chosenSvg.contents);
    });
}