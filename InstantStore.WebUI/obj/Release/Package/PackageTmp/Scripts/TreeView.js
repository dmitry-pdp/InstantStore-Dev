﻿var TreeView = function () {
    function selectCategoryTreeNodeById(id, node) {
        node = node || categoryTree[0];
        if (node.id == id) {
            return node;
        }

        if (!node.nodes || node.nodes.length == 0) {
            return null;
        }

        for (var i = 0; i < node.nodes.length; i++) {
            var r = selectCategoryTreeNodeById(id, node.nodes[i]);
            if (r) {
                return r;
            }
        }

        return null;
    };

    function initializeTreeView(id, treeId, silent) {
        if (!silent)
        {
            silent = false;
        }

        window.setTimeout(function () {
            var treeView = $('#' + treeId).treeview(true);
            var rootNode = treeView.getNode(0);
            var node = selectCategoryTreeNodeById(id, rootNode);
            if (node) {
                treeView.selectNode(node, { silent : silent });
                while (node != rootNode) {
                    treeView.expandNode(node);
                    node = treeView.getParent(node);
                }
            }
        },
        100);
    }

    return {
        InitializeTreeView: initializeTreeView
    };
}();
