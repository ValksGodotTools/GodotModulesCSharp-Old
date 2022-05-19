Player = {}
Game = {}

-- Functions
function OnGameInit() end
function OnGameUpdate() end

-- Callback
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