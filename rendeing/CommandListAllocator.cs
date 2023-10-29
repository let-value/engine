using graphics;
using Vortice.Direct3D12;

namespace rendering;

public class CommandListAllocator(CommandListFactory commandListFactory) {
    public CommandList[] Allocate(
        CommandListRequest request,
        CommandListType type,
        PipelineState? pipelineState = null
    ) {
        var totalRequired = request.GetTotalCommandListsRequired();
        var lists = new CommandList[totalRequired];

        for (uint i = 0; i < totalRequired; i++) {
            lists[i] = commandListFactory.Create(type, pipelineState);
            lists[i].Close();
        }

        return lists;
    }
}