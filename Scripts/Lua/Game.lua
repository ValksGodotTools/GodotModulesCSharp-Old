Player = {}
Game = {}

-- Functions
function OnGameInit() end
function OnGameUpdate() end

-- Callback
-- EI register callbacks to extend existing functionality or combine mods (chained function calls with pre/post joint points)
-- sample usage: RegisterCallback('OnInit', customFunction, nil)
-- sample usage with params: RegisterCallback(BasicActor.Server, 'OnHit', function (self, hit) customFunc(self, hit) end, nil)
function RegisterCallback(funcname, precallback, postcallback)
    --- fetch the original function
    local originalFunction = _G[funcname]
    --- wrap it
    _G[funcname] = function(self, ...)
        local arg = {...}

        --- call any prehook (this can change arguments but not return values)
        if precallback ~= nil then precallback(self, table.unpack(arg)) end
        --- call the original function save the result
        local result = originalFunction(self, table.unpack(arg))
        --- call any post hook, this can return a new result
        if postcallback ~= nil then postcallback(self, table.unpack(arg)) end
        -- return result
        return result
    end
end

-- EI register callbacks to extend/override existing functionality or combine mods (chained function calls with pre/post joint points)
--  this variation requires the callback to handle the return value (the result param needs to be returned)
-- sample usage:  RegisterCallbackReturnAware('OnInit', customFunction, nil)
-- sample usage with params: RegisterCallbackReturnAware(BasicActor.Server, 'OnHit', nil, function (self, ret, hit) return ret end)
--  for modification of the return value only the postcb (second one) can be used
function RegisterCallbackReturnAware(funcname, precallback, postcallback)
    local originalFunction = _G[funcname]
    _G[funcname] = function(self, ...)
        local result = nil
        if precallback ~= nil then result = precallback(self, result, table.unpack(arg)) end
        result = originalFunction(self, table.unpack(arg))
        if postcallback ~= nil then result = postcallback(self, result, table.unpack(arg)) end
        return result
    end
end