$(function () {
    var hub = $.connection.fishHub;
    var vm = new function () {
        var self = this;

        this.typeId = ko.observable();
        this.count = ko.observable(10);
        this.types = ko.observableArray();
        this.fish = ko.observableArray();
        this.loadTypes = function () {
            $.getJSON('api/FishTypes', function (data) {
                ko.utils.arrayPushAll(self.types, data);
            });
        };
        this.loadFish = function () {
            $.getJSON('api/Fish', function (data) {
                var fishes = ko.utils.arrayMap(data, function(fish) {
                    return ko.mapping.fromJS(fish);
                });
                ko.utils.arrayPushAll(self.fish, fishes);
            });
        };
        this.add = function () {
            hub.server.add({ typeId: self.typeId(), count: self.count() });
        };
        hub.client.fishUpdated = function (fishUpdate) {
            var item = ko.utils.arrayFirst(self.fish(), function(f) {
                return f.type.id() == fishUpdate.TypeId;
            });
            if (item) {
                item.count(item.count() + fishUpdate.Count);
            }
        };

    }();
    vm.loadTypes();
    vm.loadFish();
    $.connection.hub.start();
    ko.applyBindings(vm);
});
