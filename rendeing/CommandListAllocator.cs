using graphics;
using Vortice.Direct3D12;

namespace rendering;

public class CommandListAllocator(CommandListFactory commandListFactory) {
    public CommandList[] Allocate(
        int count,
        CommandListType type,
        PipelineState? pipelineState = null
    ) {
        var lists = new CommandList[count];

        for (uint i = 0; i < count; i++) {
            lists[i] = commandListFactory.Create(type, pipelineState);
            lists[i].Close();
        }

        return lists;
    }
}