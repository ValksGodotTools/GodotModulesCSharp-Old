Player = { id = 0, health = 100 }

function Player:new(o)
	o = o or {}
	setmetatable(o, self)
	self.__index = self
	return o
end

function Player:setHealth(id, v)
	self.health = v
end