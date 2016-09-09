var vtsWebapp = angular.module("vtsWebapp", []);

vtsWebapp.controller("HostsCtrl", function ($scope, $http, $timeout) {

	$(document).on('click', 'button.clear', function (e) {

		$(this).prev().find('input').val('');

		angular.element($(this).prev().find('input')).triggerHandler('input');
	});

	var dt = new Date();
	var current_month = dt.getFullYear() + '-' + String("00" + (dt.getMonth() + 1)).slice(-2);

	$scope.start_month = current_month;
	$scope.end_month = '';

	$scope.month_year = function (string) {

		if (($scope.end_month != '') && ($scope.start_month != $scope.end_month)) {
			return '*';
		}

		var m = string.charsAt([5, 6]);
		var y = string.charsAt([0, 1, 2, 3]);
		return m + '-' + y;
	}

	$scope.formatBytes = function (bytes, decimals) {

		return filesize(bytes);

	}

	$scope.getData = function (start, end) {
		console.log(end);
		$http.get('db.php', {
			params : {
				start_month : start,
				end_month : end
			}
		}).
		success(function (data, status, headers, config) {
			console.log(data);
			$scope.hosts = data;
		}).
		error(function (data, status, headers, config) {
			console.log(data);
		});
	}

	function go() {
		angular.element($('#start_month')).triggerHandler('input');
		angular.element($('#end_month')).triggerHandler('input');
	}

	$('#start_month').MonthPicker({
		StartYear : 2016,
		ShowIcon : false,
		MonthFormat : 'yy-mm',
		OnAfterChooseMonth : go
	});

	$('#end_month').MonthPicker({
		StartYear : 2016,
		ShowIcon : false,
		MonthFormat : 'yy-mm',
		OnAfterChooseMonth : go
	});

});

vtsWebapp.filter("filterByText", function () {
	return function (hosts, string) {
		return hosts.filter(function (item) {
			return item.indexOf(string) > -1;
		});
	};
});

$(document).ready(function () {});

String.prototype.charsAt = function (indexes) {
	var returned = '';
	for (var i = 0; i < indexes.length; i++) {
		returned = returned + this.charAt(indexes[i]);
	}
	return returned;
}
