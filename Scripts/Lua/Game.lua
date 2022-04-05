-- Player
Player = {}

-- Functions
function OnGameInit() end
function OnGameUpdate() end

-- Callback
function RegisterCallback(funcname, precallback, postcallback)
    --- fetch the original function
    local originalFunction = _G[funcname]
    --- wrap it
    _G[funcname] = function(self, ...)
        --- call any prehook (this can change arguments but not return values)
        if precallback ~= nil then precallback(self) end
        --- call the original function save the result
        local ret = originalFunction(self)
        --- call any post hook, this can return a new result
        if postcallback ~= nil then postcallback(self) end
        -- return result
        return ret
    end
end