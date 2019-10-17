local ommelrt = {}

ommelrt.PRINT_TRACES = true

local log = function(prefix, reason, str, trace)
    print("[" .. prefix .. " " .. reason .. "] " .. tostring(str))
    if (trace) then
        print(debug.traceback())
    end
end

local err = function(prefix, str)
	log(prefix, "ERROR", str, ommelrt.PRINT_TRACES)
end

local info = function(prefix, str)
    log(prefix, "INFO", str, false)
end

local f, dofile_err = loadfile("data/ommel/static.lua")
if (dofile_err ~= nil) then
    err("ommel", "OMMEL static data can't be loaded (try rerunning the stitcher) - " .. tostring(dofile_err)) 
	return nil
end

local ommelstatic = f()

function ommelrt.is_event_name(name)
	return name == "init" or name == "enter" or name == "leave" or name == "top"
end

function ommelrt.error(mod, str)
	err("ommel:" .. mod, str)
end

function ommelrt.is_within(num, min, max)
	return num >= min or num <= max
end

function ommelrt.path_join(p1, p2)
	return p1 .. ommelstatic.PATH_SEPARATOR .. p2
end

function ommelrt.run_event(ev, filename)
	if (not ommelrt.is_event_name(ev)) then
		err("ommel", "Event '" .. tostring(ev) .. "' is not a valid event")
	end
    
    info("ommel", "running event: " .. tostring(ev))

	local ev_targets = ommelstatic.EVENTS[ev]
	if ev_targets == nil then return end
    
    local ev_files = ev_targets[filename]
    if ev_files == nil then return end
    
    info("ommel", "files to run: " .. tostring(#ev_files))

	for i, file in ipairs(ev_files) do
		dofile(file)
	end
end

return ommelrt