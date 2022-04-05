Player = { health = 100 }

function Player:new(o)
	o = o or {}
	setmetatable(o, self)
	self.__index = self
	return o
end

function Player:setHealth(v)
	self.health = v
end